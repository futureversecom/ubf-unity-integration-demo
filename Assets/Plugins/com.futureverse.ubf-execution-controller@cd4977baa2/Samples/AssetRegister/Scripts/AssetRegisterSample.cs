// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Linq;
using AssetRegister.Runtime.Clients;
using AssetRegister.Runtime.Interfaces;
using AssetRegister.Runtime.Schema.Objects;
using AssetRegister.Runtime.Schema.Queries;
using AssetRegister.Runtime.Schema.Unions;
using Futureverse.UBF.UBFExecutionController.Runtime;
using Plugins.AssetRegister.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Futureverse.UBF.UBFExecutionController.Samples
{
	public class AssetRegisterSample : MonoBehaviour
	{
		[SerializeField] private Runtime.ExecutionController _executionController;
		[SerializeField] private MonoClient _arClient;
		[SerializeField] private string[] _supportedCollectionIds;
		
		[Header("UI")]
		[SerializeField] private AssetUI _assetUI;		
		[SerializeField] private InputField _walletInput;
		[SerializeField] private Button _searchButton;
		[SerializeField] private RectTransform _assetGrid;

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
			foreach (Transform child in _assetGrid.transform)
			{
				Destroy(child.gameObject);
			}

			StartCoroutine(FetchAssetsFromWallet(_walletInput.text, OnAssetsLoaded, OnFailure));
		}

		private void OnFailure(string obj)
		{
			// TODO: Some error text?
		}

		private void OnAssetsLoaded(Asset[] items)
		{
			foreach (var asset in items)
			{
				var assetUI = Instantiate(_assetUI, _assetGrid);
				assetUI.Load(asset, () => LoadAsset(asset));
			}
		}
		
		private void LoadAsset(Asset asset)
		{
			StartCoroutine(LoadAssetRoutine(asset));
		}

		private IEnumerator LoadAssetRoutine(Asset asset)
		{
			IInventoryItem item = null;
			yield return AssetRegisterInventoryItem.FromAsset(_arClient, asset, i => item = i);
			yield return _executionController.RenderItem(item);
		}
		
		private IEnumerator FetchAssetsFromWallet(
			string walletAddress,
			Action<Asset[]> successCallback,
			Action<string> failureCallback,
			int maxResults = 100)
		{
			var addresses = new[]
			{
				walletAddress,
			};

			IResponse response = null;
			
			yield return AR.NewQueryBuilder()
				.AddAssetsQuery(addresses:addresses, collectionIds:_supportedCollectionIds, first:maxResults)
					.OnArray<AssetEdge, AssetEdge[]>(a => a.Edges)
						.OnMember(e => e.Node)
							.WithField(n => n.Id)
							.WithField(n => n.CollectionId)
							.WithField(n => n.TokenId)
							.WithField(n => n.Profiles)
							.OnMember(n => n.Metadata)
								.WithField(m => m.Properties)
								.WithField(m => m.Attributes)
								.WithField(m => m.RawAttributes)
								.Done()
							.OnUnion(n => n.Links)
								.On<NFTAssetLink>()
									.OnArray<Link, Link[]>(nft => nft.ChildLinks)
										.WithField(l => l.Path)
										.OnMember(l => l.Asset)
											.WithField(a => a.Id)
											.WithField(a => a.CollectionId)
											.WithField(a => a.TokenId)
				.Execute(_arClient, r => response = r);

			if (!response.Success)
			{
				failureCallback?.Invoke($"Asset Register request had errors: {response.Error}");
				yield break;
			}

			if (!response.TryGetResult(out AssetsResult result))
			{
				failureCallback?.Invoke($"Asset Register request had errors: {response.Error}");
				yield break;
			}

			var assets = result.Assets.Edges.Select(e => e.Node).ToArray();
			successCallback?.Invoke(assets);
		}
	}
}