// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetRegister.Runtime.Clients;
using AssetRegister.Runtime.Interfaces;
using AssetRegister.Runtime.Schema.Objects;
using AssetRegister.Runtime.Schema.Queries;
using AssetRegister.Runtime.Schema.Unions;
using Futureverse.UBF.Runtime.Execution;
using Plugins.AssetRegister.Runtime;
using UnityEngine;
using UnityEngine.Events;

namespace Futureverse.UBF.UBFExecutionController.Runtime
{
	public class ExecutionController : MonoBehaviour
	{
		[SerializeField] private MonoClient _arClient;
		[SerializeField] private UBFRuntimeController _ubfController;
		
		[Header("Settings")]
		[SerializeField] private string[] _supportedCollectionIds;

		[Header("Events")]
		[SerializeField] private UnityEvent<Asset[]> _onFetchAssetsSuccess;
		[SerializeField] private UnityEvent<string> _onFetchAssetsFailure;

		private readonly Dictionary<string, Asset> _loadedAssets = new();

		public void ClearFetchedAssets()
		{
			_loadedAssets.Clear();
		}
		
		/// <summary>
		/// Attempts to retrieve all assets in a wallet with the supported collection IDs serialized on this component.
		/// </summary>
		/// <param name="walletAddress">The wallet to load the Assets from</param>
		/// <param name="successCallback">Called if Assets were successfully retrieved. Contains an array of retrieved Assets</param>
		/// <param name="failureCallback">Called if Asset retrieval failed. Contains an error string</param>
		/// <param name="maxResults"></param>
		/// <returns></returns>
		public IEnumerator FetchAssetsFromWallet(
			string walletAddress,
			Action<Asset[]> successCallback,
			Action<string> failureCallback,
			int maxResults = 100)
		{
			ClearFetchedAssets();
			
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
							.OnMember(n => n.Collection)
								.WithField(c => c.ChainID)
								.WithField(c => c.ChainType)
								.WithField(c => c.Location)
								.Done()
							.OnUnion(n => n.Links)
								.On<NFTAssetLink>()
									.OnArray<Link, Link[]>(nft => nft.ChildLinks)
										.WithField(l => l.Asset.Id)
										.WithField(l => l.Path)
				.Execute(_arClient, r => response = r);

			if (!response.Success)
			{
				_onFetchAssetsFailure?.Invoke($"Asset Register request had errors: {response.Error}");
				failureCallback?.Invoke($"Asset Register request had errors: {response.Error}");
				yield break;
			}

			if (!response.TryGetResult(out AssetsResult result))
			{
				_onFetchAssetsFailure?.Invoke($"Could not get Assets result from response");
				failureCallback?.Invoke($"Asset Register request had errors: {response.Error}");
				yield break;
			}
			
			var assets = result.Assets.Edges.Select(e => e.Node).ToArray();
			foreach (var asset in assets)
			{
				_loadedAssets.Add(asset.Id, asset);
			}
			
			_onFetchAssetsSuccess.Invoke(assets);
			successCallback?.Invoke(assets);
		}

		/// <summary>
		/// Given an Asset Register Asset, creates an asset tree and executes it using UBF.
		/// </summary>
		/// <param name="asset">The Asset to render</param>
		/// <param name="parser"></param>
		/// <returns></returns>
		public IEnumerator LoadUBFAsset(Asset asset, IAssetParser parser = null)
		{
			parser ??= new DefaultAssetParser();

			List<IBlueprintInstanceData> blueprints = null;
			IArtifactProvider artifactProvider = null;
			string rootId = null;

			yield return parser.ParseAsset(
				_loadedAssets,
				asset.Id,
				(x) =>
				{
					blueprints = x.Item1;
					artifactProvider = x.Item2;
					rootId = x.Item3;
				}
			);

			yield return _ubfController.Execute(rootId, artifactProvider, blueprints);
		}
	}
}