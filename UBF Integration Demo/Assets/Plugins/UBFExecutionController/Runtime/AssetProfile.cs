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
	// Asset Profile JSON format -   Asset Name,        Variant Name,      Version
	using AssetProfiles = Dictionary<string, Dictionary<string, Dictionary<string, AssetProfile.AssetProfileData>>>;

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

		public string RenderBlueprintResourceId { get; private set; }
		public string ParsingBlueprintResourceId { get; private set; }
		public Catalog RenderCatalog { get; private set; }
		public Catalog ParsingCatalog { get; private set; }

		public static IEnumerator FetchByAssetId(string collectionId, string assetName, Action<AssetProfile> onComplete)
		{
			var settings = ExecutionControllerSettings.GetOrCreateSettings();
			var remotePath = $"{settings.AssetProfilesPath}/profiles_{collectionId}.json";
			yield return FetchByUri(remotePath, assetName, onComplete);
		}

		public static IEnumerator FetchByUri(string uri, string assetName, Action<AssetProfile> onComplete)
		{
			// Fetch asset profile data
			var resourceHandler = new ResourceLoader<AssetProfiles>(
				new BasicResource(uri),
				new DefaultDownloader(),
				new JsonLoader<AssetProfiles>()
			);

			AssetProfiles profileCollectionData = null;
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

			// TODO Get supported variants?
			var defaultVariant = profile["Default"];
			var validVersions = defaultVariant.Keys.Select(BlueprintVersion.FromString)
				.Where(v => v != null && v.IsSupported())
				.ToList();
			validVersions.Sort((a, b) => b.CompareTo(a));
			var version = validVersions.First()
					?.ToString() ??
				"";

			if (!defaultVariant.TryGetValue(version, out var value))
			{
				Debug.LogError($"No asset profile of a supported version found for {assetName}.");
				onComplete?.Invoke(null);
				yield break;
			}

			yield return FromProfileData(value, onComplete);
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