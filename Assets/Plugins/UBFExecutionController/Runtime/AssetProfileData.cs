// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Futureverse.UBF.ExecutionController.Runtime
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
	
	[JsonObject]
	public class ProfileQueryVariables
	{
		[JsonProperty("assetIds")] public ProfileQueryFilter[] AssetIds;
	}
	
	[JsonObject]
	public class ProfileQueryFilter
	{
		[JsonProperty("collectionId")] public string CollectionId;
		[JsonProperty("tokenId")] public string TokenId;
	}

	[JsonObject]
	public class QueryResponse
	{
		[JsonProperty("data")] public QueryResponseData Data;
	}
	
	[JsonObject]
	public class QueryResponseData
	{
		[JsonProperty("assetsByIds")] public QueryResponseProfiles[] Profiles;
	}
	
	[JsonObject]
	public class QueryResponseProfiles
	{
		[JsonProperty("profiles")] public QueryResponseAssetProfile Profile;
	}
	
	[JsonObject]
	public class QueryResponseAssetProfile
	{
		[JsonProperty("asset-profile")] public string Url;
	}
}