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
using Futureverse.Sylo;
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

		[Header("Settings"), SerializeField, Tooltip("If true, attempt to load any child asset if it is not already loaded")] 
		private bool _enableForwardLoading;
		[SerializeField] private string _syloResolverUri;
		[SerializeField, Tooltip("If false, all collections are supported")]
		private bool _specifySupportedCollectionIds;
		[SerializeField] private string[] _supportedCollectionIds;
		[SerializeField, Tooltip("If false, supported variants are dictated by the Execution Controller settings")]
		private bool _overrideSupportedVariants;
		[SerializeField] private string[] _variantOverrides;
		
		[SerializeField, Header("Events")] private UnityEvent<Asset[]> _onFetchAssetsSuccess;
		[SerializeField] private UnityEvent<string> _onFetchAssetsFailure;

		private readonly Dictionary<string, Asset> _loadedAssets = new();

		private void Start()
		{
			SyloUtilities.SetResolverUri(_syloResolverUri);
		}

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

			var collections = _specifySupportedCollectionIds ? _supportedCollectionIds : Array.Empty<string>();
			var addresses = new[]
			{
				walletAddress,
			};

			IResponse response = null;
			
			yield return AR.NewQueryBuilder()
				.AddAssetsQuery(addresses:addresses, collectionIds:collections, first:maxResults)
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
										.WithField(l => l.Path)
										.OnMember(l => l.Asset)
											.WithField(a => a.Id)
											.WithField(a => a.CollectionId)
											.WithField(a => a.TokenId)
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
		/// <returns></returns>
		public IEnumerator LoadUBFAsset(Asset asset)
		{
			var artifactProvider = new FutureverseArtifactProvider();
			var blueprints = new List<IBlueprintInstanceData>();

			string rootId = null;
			yield return PrepareAssetRecursive(
				_loadedAssets,
				asset,
				blueprints,
				artifactProvider,
				i => rootId = i.InstanceId
			);

			if (rootId == null)
			{
				Debug.LogError($"Could not load root asset. Aborting");
				yield break;
			}
			
			yield return _ubfController.Execute(rootId, artifactProvider, blueprints);
		}
		
		private IEnumerator PrepareAssetRecursive(
			Dictionary<string, Asset> loadedAssets,
			Asset asset,
			List<IBlueprintInstanceData> blueprintInstances,
			ArtifactProvider artifactProvider,
			Action<IBlueprintInstanceData> callback)
		{
			AssetProfile profile = null;
			yield return AssetProfile.FetchByAsset(
				asset,
				p => profile = p,
				_overrideSupportedVariants ? _variantOverrides : null
			);
			
			if (profile == null)
			{
				Debug.LogError($"Failed to fetch asset profile for {asset.GetFullIdentifier()}");
				yield break;
			}
			
			if (profile.RenderCatalog == null)
			{
				Debug.LogError($"Render catalog is null for {asset.GetFullIdentifier()}");
				yield break;
			}
			
			artifactProvider.RegisterCatalog(profile.RenderCatalog);
			var renderBlueprintDefinition = new BlueprintInstanceData(profile.RenderBlueprintResourceId);
			
			yield return GetParsingInputs(profile, asset.GetFullMetadata(), renderBlueprintDefinition);

			if (asset.Links is NFTAssetLink nftLink)
			{
				foreach (var link in nftLink.ChildLinks)
				{
					var path = link.Path.Split("#")[^1]
						.Replace("_accessory", "");

					if (!loadedAssets.TryGetValue(link.Asset.Id, out var linkedAsset))
					{
						if (!_enableForwardLoading)
						{
							Debug.LogWarning($"Skipping child {link.Asset.GetFullIdentifier()} - asset is not loaded, and forward loading is not enabled");
							continue;
						}

						yield return ForwardLoadAsset(
							link.Asset.CollectionId,
							link.Asset.TokenId,
							a => linkedAsset = a
						);
					}
					
					if (linkedAsset == null)
					{
						Debug.LogWarning($"Skipping child {link.Asset.GetFullIdentifier()} - failed to forward load child asset");
						continue;
					}

					yield return PrepareAssetRecursive(
						loadedAssets,
						linkedAsset,
						blueprintInstances,
						artifactProvider,
						i => renderBlueprintDefinition.AddInput(path, i.InstanceId)
					);
				}
			}
			
			blueprintInstances.Add(renderBlueprintDefinition);
			callback?.Invoke(renderBlueprintDefinition);
		}

		private static IEnumerator GetParsingInputs(AssetProfile profile, string metadata, BlueprintInstanceData renderBlueprint)
		{
			if (profile.ParsingCatalog == null || string.IsNullOrEmpty(profile.ParsingBlueprintResourceId))
			{
				yield break;
			}
			
			var parsingBlueprintDefinition = new BlueprintInstanceData(profile.ParsingBlueprintResourceId);
			parsingBlueprintDefinition.AddInput("metadata", metadata);
			
			var parsingArtifactProvider = new ArtifactProvider();
			parsingArtifactProvider.RegisterCatalog(profile.ParsingCatalog);

			yield return UBFExecutor.ExecuteRoutine(
				new ExecutionData(
					null,
					result =>
					{
						foreach (var output in result.BlueprintOutputs)
						{
							renderBlueprint.AddInput(output.Key, output.Value);
						}
					},
					new List<IBlueprintInstanceData>
					{
						parsingBlueprintDefinition,
					},
					parsingArtifactProvider
				),
				parsingBlueprintDefinition.InstanceId
			);
		}

		private IEnumerator ForwardLoadAsset(string collectionId, string tokenId, Action<Asset> callback)
		{
			if (_specifySupportedCollectionIds && !_supportedCollectionIds.Contains(collectionId))
			{
				yield break;
			}
			
			IResponse response = null;
			yield return AR.NewQueryBuilder()
				.AddAssetQuery(collectionId, tokenId)
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
								.WithField(l => l.Path)
								.OnMember(l => l.Asset)
									.WithField(a => a.Id)
									.WithField(a => a.CollectionId)
									.WithField(a => a.TokenId)
				.Execute(_arClient, r => response = r);

			if (!response.Success || !response.TryGetResult(out AssetResult result))
			{
				Debug.LogError($"Error forward loading Asset - {response.Error}");
				yield break;
			}

			callback?.Invoke(result.Asset);
		}
	}
}