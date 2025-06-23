// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.ExecutionController.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Testbed.AssetRegister
{
	public class AssetRegisterExecutor : MonoBehaviour
	{
		[SerializeField] private string[] _collectionIds;
		[SerializeField] private bool _overrideSupportedVariants;
		[SerializeField] private string[] _supportedVariantOverrides;
		[SerializeField] private int _numResults;
		[SerializeField] private FutureverseRuntimeController _controller;
		[SerializeField] private AssetUI _assetUIPrefab;
		[SerializeField] private InputField _walletInput;
		[SerializeField] private Button _searchButton;
		[SerializeField] private RectTransform _assetGrid;

		private Dictionary<string, InventoryNode> _inventoryNodes = new();
		
		private void Start()
		{
			_searchButton.onClick.AddListener(OnWalletEntered);
		}

		private void OnWalletEntered()
		{
			if (_walletInput.text == string.Empty)
			{
				return;
			}
			
			StopAllCoroutines();
			_inventoryNodes.Clear();
			foreach (Transform child in _assetGrid.transform)
			{
				Destroy(child.gameObject);
			}
			
			Resources.UnloadUnusedAssets();
			
			StartCoroutine(AssetRegisterQuery.InventoryQueryRoutine(
				_walletInput.text,
				_collectionIds,
				_numResults,
				OnWalletsLoaded
			));
		}

		private void OnWalletsLoaded(bool success, InventoryNode[] assets)
		{
			if (!success)
			{
				// TODO: Set some error text
			}

			foreach (var asset in assets)
			{
				_inventoryNodes.Add(asset.Id, asset);
				var assetUI = Instantiate(_assetUIPrefab, _assetGrid);
				assetUI.Load(asset, () => LoadAsset(asset));
			}
		}

		private void LoadAsset(InventoryNode asset)
		{
			var (assetTree, assets) = CreateTreeForAsset(asset);
			var dataParser = new AssetProfileDataParser(_overrideSupportedVariants ? _supportedVariantOverrides : null);
			StartCoroutine(_controller.ExecuteAssetTree(assetTree, dataParser, null));
		}

		private (IUbfTree, IUbfData[]) CreateTreeForAsset(InventoryNode asset)
		{
			var assetTree = new ARAssetTree(asset);
			Dictionary<string, IUbfData> children = new();
			foreach (var keypair in asset.EquippedAssets)
			{
				if (_inventoryNodes.TryGetValue(keypair.Value, out var child))
				{
					children.Add(keypair.Key, child);
				}
			}
			assetTree.RegisterElement(asset, children);
			var allAssets = new List<IUbfData>
			{
				asset,
			};
			
			// TODO: Make recursive when we have collections that require it
			foreach (var assetId in asset.EquippedAssets.Values)
			{
				// This ignores child assets belonging to any collections NOT defined in the _collectionIds array
				if (!_collectionIds.Select(id => id.Split(":")[^1])
					.Contains(assetId.Split(":")[0]))
				{
					continue;
				}
				
				if (!_inventoryNodes.TryGetValue(assetId, out var node))
				{
					continue;
				}
				
				allAssets.Add(node);
				Dictionary<string, IUbfData> c = new();
				foreach (var keypair in asset.EquippedAssets)
				{
					if (_inventoryNodes.TryGetValue(keypair.Value, out var child))
					{
						c.Add(keypair.Key, child);
					}
				}
				assetTree.RegisterElement(node, c);
			}

			return (assetTree, allAssets.ToArray());
		}
	}
}