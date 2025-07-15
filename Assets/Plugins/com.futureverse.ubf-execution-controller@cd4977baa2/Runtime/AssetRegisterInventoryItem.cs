// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using AssetRegister.Runtime.Interfaces;
using AssetRegister.Runtime.Schema.Objects;
using AssetRegister.Runtime.Schema.Queries;
using AssetRegister.Runtime.Schema.Unions;
using Futureverse.UBF.ExecutionController.Runtime.Settings;
using Newtonsoft.Json.Linq;
using Plugins.AssetRegister.Runtime;
using UnityEngine;

namespace Futureverse.UBF.UBFExecutionController.Runtime
{
	public class AssetRegisterInventoryItem : IInventoryItem
	{
		public string Id { get; private set; }
		public string Name => $"{CollectionId}:{TokenId}";
		public string CollectionId { get; private set; }
		public string TokenId { get; private set; }
		public AssetProfile AssetProfile { get; private set; }
		public JObject Metadata { get; private set; }
		public Dictionary<string, IInventoryItem> Children { get; private set; }
		
		private static readonly Dictionary<string, AssetRegisterInventoryItem> s_cachedInventoryItems = new();

		public static IEnumerator FromData(
			IClient client,
			string collectionId,
			string tokenId,
			Action<AssetRegisterInventoryItem> callback)
		{
			IResponse response = null;
			yield return AR.NewQueryBuilder()
				.AddAssetQuery(collectionId, tokenId)
					.WithField(n => n.Id)
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
				.Execute(client, r => response = r);

			if (!response.Success)
			{
				Debug.LogError(response.Error);
				yield break;
			}

			if (!response.TryGetResult(out AssetResult result))
			{
				Debug.LogError("Could not get Asset from query response");
				yield break;
			}

			var asset = result.Asset;
			asset.CollectionId = collectionId;
			asset.TokenId = tokenId;
			
			yield return FromAsset(client, asset, callback);
		}
		
		public static IEnumerator FromAsset(IClient client, Asset asset, Action<AssetRegisterInventoryItem> callback)
		{
			if (s_cachedInventoryItems.TryGetValue(asset.Id, out var inventoryAsset))
			{
				callback?.Invoke(inventoryAsset);
				yield break;
			}
			
			inventoryAsset = new AssetRegisterInventoryItem();
			
			if (asset.Id == null)
			{
				Debug.LogError("Asset is missing field 'Id'. You must include this in the Asset Register query.");
				yield break;
			}
			inventoryAsset.Id = asset.Id;
			
			if (asset.CollectionId == null)
			{
				Debug.LogError("Asset is missing field 'CollectionId'. You must include this in the Asset Register query.");
				yield break;
			}
			inventoryAsset.CollectionId = asset.CollectionId;
			
			if (asset.TokenId == null)
			{
				Debug.LogError("Asset is missing field 'TokenId'. You must include this in the Asset Register query.");
				yield break;
			}
			inventoryAsset.TokenId = asset.TokenId;

			yield return RetrieveMissingData(
				client,
				asset,
				(metadata, profiles, link) =>
				{
					asset.Metadata ??= metadata;
					asset.Profiles ??= profiles;
					asset.Links ??= link;
				}
			);
			
			var settings = ExecutionControllerSettings.GetOrCreateSettings();
			if (settings.UseAssetRegisterProfiles)
			{
				if (asset.Profiles != null && asset.Profiles.TryGetValue("asset-profile", out var profile))
				{
					yield return AssetProfile.FetchByUri(
						profile.ToString(),
						inventoryAsset.Name,
						p => inventoryAsset.AssetProfile = p
					);
				}
			}
			else
			{
				var collectionLocation = asset.CollectionId.Split(":")[^1];
				var assetProfileUrl = $"{settings.AssetProfilesPath}/{collectionLocation}.json";
				yield return AssetProfile.FetchByUriLegacy(
					assetProfileUrl,
					asset.TokenId,
					p => inventoryAsset.AssetProfile = p
				);
			}
			
			inventoryAsset.Metadata = new JObject
			{
				{
					"metadata", new JObject
					{
						{
							"properties", asset.Metadata?.Properties ?? ""
						},
						{
							"attributes", asset.Metadata?.Attributes ?? ""
						},
						{
							"rawAttributes", asset.Metadata?.RawAttributes ?? ""
						},
					}
				},
			};
			
			inventoryAsset.Children = new Dictionary<string, IInventoryItem>();
			if (asset.Links is NFTAssetLink nftLink)
			{
				foreach (var link in nftLink.ChildLinks)
				{
					var path = link.Path.Split("#")[^1]
						.Replace("_accessory", "");

					if (!s_cachedInventoryItems.TryGetValue(link.Asset.Id, out var child))
					{
						yield return FromData(
							client,
							link.Asset.CollectionId,
							link.Asset.TokenId,
							a => child = a
						);
					}

					if (child != null)
					{
						inventoryAsset.Children.Add(path, child);
					}
				}
			}

			s_cachedInventoryItems.TryAdd(inventoryAsset.Id, inventoryAsset);
			callback?.Invoke(inventoryAsset);
		}
		
		private static IEnumerator RetrieveMissingData(IClient client, Asset asset, Action<Metadata, JObject, AssetLink> callback)
		{
			IMemberSubBuilder<IQueryBuilder, Asset> queryBuilder = null;
			
			if (asset.Profiles == null)
			{
				queryBuilder = AR.NewQueryBuilder()
					.AddAssetQuery(asset.CollectionId, asset.TokenId)
						.WithField(a => a.Profiles);
			}
			
			if (asset.Links == null)
			{
				queryBuilder ??= AR.NewQueryBuilder()
					.AddAssetQuery(asset.CollectionId, asset.TokenId);

				queryBuilder = queryBuilder
					.OnUnion(n => n.Links)
						.On<NFTAssetLink>()
							.OnArray<Link, Link[]>(nft => nft.ChildLinks)
								.WithField(l => l.Path)
								.OnMember(l => l.Asset)
									.WithField(a => a.Id)
									.WithField(a => a.CollectionId)
									.WithField(a => a.TokenId)
									.Done()
								.Done()
							.Done()
						.Done();
			}
			
			if (asset.Metadata == null)
			{
				queryBuilder ??= AR.NewQueryBuilder()
					.AddAssetQuery(asset.CollectionId, asset.TokenId);

				queryBuilder = queryBuilder
					.OnMember(n => n.Metadata)
						.WithField(m => m.Properties)
						.WithField(m => m.Attributes)
						.WithField(m => m.RawAttributes)
						.Done();
			}

			if (queryBuilder == null)
			{
				yield break;
			}

			IResponse response = null;
			yield return queryBuilder.Execute(client, r => response = r);

			if (!response.Success)
			{
				Debug.LogError(response.Error);
				yield break;
			}

			if (!response.TryGetResult(out AssetResult result))
			{
				Debug.LogError("Could not get Asset from query response");
				yield break;
			}
			
			callback?.Invoke(result.Asset.Metadata, result.Asset.Profiles, result.Asset.Links);
		}
	}
}