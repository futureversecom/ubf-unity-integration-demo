// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.ExecutionController.Runtime.Settings;
using Futureverse.UBF.Runtime;
using Futureverse.UBF.Runtime.Resources;
using Newtonsoft.Json;
using UnityEngine;

namespace Futureverse.UBF.ExecutionController.Runtime
{
	public class AssetProfile
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

		public string RenderBlueprintResourceId { get; private set; }
		public string ParsingBlueprintResourceId { get; private set; }
		public Catalog RenderCatalog { get; private set; }
		public Catalog ParsingCatalog { get; private set; }

		public static IEnumerator FetchByAssetId(
			string collectionId,
			string assetName,
			Action<AssetProfile> onComplete,
			string[] variantsOverride = null)
		{
			var settings = ExecutionControllerSettings.GetOrCreateSettings();
			var remotePath = $"{settings.AssetProfilesPath}/{collectionId}.json";
			yield return FetchByUri(remotePath, assetName, onComplete, variantsOverride);
		}

		public static IEnumerator FetchByUri(
			string uri,
			string assetName,
			Action<AssetProfile> onComplete,
			string[] variantsOverride = null)
		{
			// Fetch asset profile data
			var resourceHandler = new ResourceLoader<Dictionary<string, AssetProfileJson>>(
				new BasicResource(uri),
				new DefaultDownloader(),
				new JsonLoader<Dictionary<string, AssetProfileJson>>()
			);

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
				profile = profileCollectionData[assetName];
			}

			if (profile == null)
			{
				Debug.LogError($"No asset profile found with asset name {assetName}, and no override profile provided");
				onComplete?.Invoke(null);
				yield break;
			}

			var profileData = GetProfileData(profile, variantsOverride);
			if (profileData == null)
			{
				Debug.LogError($"No asset profile with supported variant and version found for {assetName}.");
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
			var profile = new AssetProfile();
			var downloader = new DefaultDownloader();

			profile.RenderBlueprintResourceId = profileData.RenderBlueprintId;
			profile.ParsingBlueprintResourceId = profileData.ParsingBlueprintId;

			if (!string.IsNullOrEmpty(profileData.RenderCatalogUri))
			{
				var renderCatalogLoader = new ResourceLoader<Catalog>(
					new BasicResource(profileData.RenderCatalogUri),
					downloader,
					new JsonLoader<Catalog>()
				);
				yield return renderCatalogLoader.Get(data => profile.RenderCatalog = data);
			}

			if (!string.IsNullOrEmpty(profileData.ParsingCatalogUri))
			{
				var parsingCatalogLoader = new ResourceLoader<Catalog>(
					new BasicResource(profileData.ParsingCatalogUri),
					downloader,
					new JsonLoader<Catalog>()
				);
				yield return parsingCatalogLoader.Get(data => profile.ParsingCatalog = data);
			}

			onComplete?.Invoke(profile);
		}
	}
}