// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Futureverse.UBF.ExecutionController.Runtime.Settings;
using Futureverse.UBF.Runtime;
using Futureverse.UBF.Runtime.Resources;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Futureverse.UBF.ExecutionController.Runtime
{
	public class AssetProfile
	{
		public string RenderBlueprintResourceId { get; private set; }
		public string ParsingBlueprintResourceId { get; private set; }
		public Catalog RenderCatalog { get; private set; }
		public Catalog ParsingCatalog { get; private set; }

		public static IEnumerator FetchByAssetId(
			string chainId,
			string chainName,
			string collectionId,
			string tokenId,
			Action<AssetProfile> onComplete,
			string[] variantsOverride = null)
		{
			var settings = ExecutionControllerSettings.GetOrCreateSettings();
			if (settings.UseAssetRegisterProfiles)
			{
				var fullCollectionId = $"{chainId}:{chainName}:{collectionId}";
				string assetProfileUrl = null;
				yield return GetAssetProfileUrlFromAssetRegister(fullCollectionId, tokenId, (url) => assetProfileUrl = url);
				if (assetProfileUrl == null)
				{
					onComplete?.Invoke(null);
					yield break;
				}
				
				yield return FetchByUri(assetProfileUrl, $"{collectionId}:{tokenId}", onComplete, variantsOverride);
			}
			else
			{
				var assetProfileUrl = $"{settings.AssetProfilesPath}/{collectionId}.json";
				yield return FetchByUriLegacy(assetProfileUrl, tokenId, onComplete, variantsOverride);
			}
			
		}

		private static IEnumerator GetAssetProfileUrlFromAssetRegister(string collectionId, string tokenId, Action<string> onComplete)
		{
			const string url = "https://ar-api.futureverse.cloud/graphql";
			const string query = @"
				query Assets($assetIds: [AssetInput!]) {
				  assetsByIds(assetIds: $assetIds) {
				    profiles
				  }
				}";
			var variables = new ProfileQueryVariables()
			{
				AssetIds = new[]
				{
					new ProfileQueryFilter()
					{
						CollectionId = collectionId,
						TokenId = tokenId,
					},
				},
			};
			
			var payload = new { query, variables };
			var jsonPayload = JsonConvert.SerializeObject(payload, Formatting.None);
			
			using var webRequest = new UnityWebRequest(url, "POST");
          
			webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
			webRequest.downloadHandler = new DownloadHandlerBuffer();
			webRequest.SetRequestHeader("Content-Type", "application/json");

			yield return webRequest.SendWebRequest();

			if (webRequest.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"GraphQL request failed: {webRequest.error}");
				onComplete?.Invoke(null);
				yield break;
			}
			
			var resultString = webRequest.downloadHandler.text;
			var resultData = JsonConvert.DeserializeObject<QueryResponse>(resultString);
			var profiles = resultData?.Data?.Profiles;
			if (profiles == null || profiles.Length == 0)
			{
				onComplete?.Invoke(null);
				yield break;
			}
			onComplete?.Invoke(profiles[0]?.Profile?.Url);
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
			// Fetch asset profile data
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
			var profile = new AssetProfile();
			var downloader = new DefaultDownloader();

			profile.RenderBlueprintResourceId = profileData.RenderBlueprintId;
			profile.ParsingBlueprintResourceId = profileData.ParsingBlueprintId;

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