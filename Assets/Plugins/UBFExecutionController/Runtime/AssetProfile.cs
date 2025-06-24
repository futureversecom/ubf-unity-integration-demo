// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetRegister.Runtime.Schema.Objects;
using Futureverse.UBF.ExecutionController.Runtime.Settings;
using Futureverse.UBF.Runtime;
using Futureverse.UBF.Runtime.Resources;
using Newtonsoft.Json;
using UnityEngine;

namespace Futureverse.UBF.UBFExecutionController.Runtime
{
	[JsonObject]
	public class AssetProfileData
	{
		[JsonProperty(PropertyName = "render-instance")]
		public readonly string RenderBlueprintId;
		[JsonProperty(PropertyName = "render-catalog")]
		public readonly string RenderCatalogUri;
		[JsonProperty(PropertyName = "parsing-instance")]
		public readonly string ParsingBlueprintId;
		[JsonProperty(PropertyName = "parsing-catalog")]
		public readonly string ParsingCatalogUri;
	}

	[JsonObject]
	public class AssetProfileJson
	{
		[JsonProperty(PropertyName = "profile-version")]
		public string ProfileVersion;
		[JsonProperty(PropertyName = "ubf-variants")]
		public Dictionary<string, Dictionary<string, AssetProfileData>> Variants;
	}
	
	public class AssetProfile
	{
		public string RenderBlueprintResourceId { get; private set; }
		public string ParsingBlueprintResourceId { get; private set; }
		public Catalog RenderCatalog { get; private set; }
		public Catalog ParsingCatalog { get; private set; }

		public static IEnumerator FetchByAsset(
			Asset asset,
			Action<AssetProfile> onComplete,
			string[] variantsOverride = null)
		{
			var settings = ExecutionControllerSettings.GetOrCreateSettings();
			if (settings.UseAssetRegisterProfiles)
			{
				if (!asset.Profiles.TryGetValue("asset-profile", out var profile))
				{
					Debug.LogError($"No asset profile exists on Asset Register for asset {asset.CollectionId}:{asset.TokenId}");
					yield break;
				}
				
				yield return FetchByUri(profile.ToString(), $"{asset.CollectionId}:{asset.TokenId}", onComplete, variantsOverride);
			}
			else
			{
				var assetProfileUrl = $"{settings.AssetProfilesPath}/{asset.Collection.Location}.json";
				yield return FetchByUriLegacy(assetProfileUrl, asset.TokenId, onComplete, variantsOverride);
			}
		}

		public static IEnumerator FetchByUri(	
			string uri,
			string fullId,
			Action<AssetProfile> onComplete,
			string[] variantsOverride = null)
		{
			var resourceHandler = new JsonResourceLoader<AssetProfileJson>(uri);

			AssetProfileJson profile = null;
			yield return resourceHandler.Get(data => profile = data);
			if (profile == null)
			{
				Debug.LogError($"No asset profile found for {fullId}");
				onComplete?.Invoke(null);
				yield break;
			}

			var profileData = GetProfileData(profile, variantsOverride);
			if (profileData == null)
			{
				Debug.LogError($"No asset profile with supported variant and version found for {fullId}.");
				yield break;
			}

			yield return FromProfileData(profileData, onComplete);
		}
		
		// This will be removed next release once all profiles are uploaded to AR and confirmed working
		public static IEnumerator FetchByUriLegacy(
			string uri,
			string tokenId,
			Action<AssetProfile> onComplete,
			string[] variantsOverride = null)
		{
			var resourceHandler = new JsonResourceLoader<Dictionary<string, AssetProfileJson>>(uri);

			Dictionary<string, AssetProfileJson> profileCollectionData = null;
			yield return resourceHandler.Get(data => profileCollectionData = data);
			if (profileCollectionData == null)
			{
				Debug.LogError($"No asset profile collection found at url {uri}");
				onComplete?.Invoke(null);
				yield break;
			}

			// Need to check if override profile exists
			if (!profileCollectionData.TryGetValue("override", out var profile))
			{
				profile = profileCollectionData[tokenId];
			}

			if (profile == null)
			{
				Debug.LogError($"No asset profile found with asset name {tokenId}, and no override profile provided");
				onComplete?.Invoke(null);
				yield break;
			}

			var profileData = GetProfileData(profile, variantsOverride);
			if (profileData == null)
			{
				Debug.LogError($"No asset profile with supported variant and version found for {tokenId}.");
				yield break;
			}

			yield return FromProfileData(profileData, onComplete);
		}

		private static AssetProfileData GetProfileData(AssetProfileJson profile, string[] variantsOverride = null)
		{
			var supportedVariants = variantsOverride ??
				ExecutionControllerSettings.GetOrCreateSettings()
					.SupportedVariants;

			Dictionary<string, AssetProfileData> variant = null;
			foreach (var v in supportedVariants)
			{
				if (profile.Variants.TryGetValue(v, out var variantData))
				{
					variant = variantData;
					break;
				}
			}

			if (variant == null)
			{
				return null;
			}
			
			var validVersions = variant.Keys.Select(Version.Parse)
				.Where(v => v != null && v.IsSupported())
				.ToList();
			
			validVersions.Sort((a, b) => b.CompareTo(a));
			var version = validVersions.FirstOrDefault()
					?.ToString() ??
				"";

			return variant.GetValueOrDefault(version);
		}

		private static IEnumerator FromProfileData(AssetProfileData profileData, Action<AssetProfile> onComplete)
		{
			var profile = new AssetProfile
			{
				RenderBlueprintResourceId = profileData.RenderBlueprintId,
				ParsingBlueprintResourceId = profileData.ParsingBlueprintId,
			};

			if (!string.IsNullOrEmpty(profileData.RenderCatalogUri))
			{
				var renderCatalogLoader = new JsonResourceLoader<Catalog>(profileData.RenderCatalogUri);
				yield return renderCatalogLoader.Get(data => profile.RenderCatalog = data);
			}

			if (!string.IsNullOrEmpty(profileData.ParsingCatalogUri))
			{
				var parsingCatalogLoader = new JsonResourceLoader<Catalog>(profileData.ParsingCatalogUri);
				yield return parsingCatalogLoader.Get(data => profile.ParsingCatalog = data);
			}

			onComplete?.Invoke(profile);
		}
	}
}