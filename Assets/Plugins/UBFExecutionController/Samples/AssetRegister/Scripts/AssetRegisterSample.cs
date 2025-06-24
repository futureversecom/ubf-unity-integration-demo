// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using AssetRegister.Runtime.Schema.Objects;
using UnityEngine;
using UnityEngine.UI;

namespace Futureverse.UBF.UBFExecutionController.Samples
{
	public class AssetRegisterSample : MonoBehaviour
	{
		[SerializeField] private Runtime.ExecutionController _executionController;
		
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

			StartCoroutine(_executionController.FetchAssetsFromWallet(_walletInput.text, OnAssetsLoaded, OnFailure));
		}

		private void OnFailure(string obj)
		{
			// TODO: Some error text?
		}

		private void OnAssetsLoaded(Asset[] assets)
		{
			foreach (var asset in assets)
			{
				var assetUI = Instantiate(_assetUI, _assetGrid);
				assetUI.Load(asset, () => LoadAsset(asset));
			}
		}
		
		private void LoadAsset(Asset asset)
		{
			StartCoroutine(_executionController.LoadUBFAsset(asset));
		}
	}
}