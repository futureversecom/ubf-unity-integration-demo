// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using AssetRegister.Runtime.Schema.Objects;
using Newtonsoft.Json.Linq;

namespace Futureverse.UBF.UBFExecutionController.Runtime
{
	public static class AssetExtensions
	{
		/// <summary>
		/// Wraps the properties, attributes, and rawAttributes fields from the Asset's metadata in a way that can
		/// be interpreted by general parsing Blueprints. Pass this in as 'metadata' input to any parsing Blueprint
		/// </summary>
		/// <param name="asset"></param>
		/// <returns>The full metadata for this asset</returns>
		public static string GetFullMetadata(this Asset asset)
		{
			var subMeta = new JObject
			{
				{
					"properties", asset?.Metadata?.Properties ?? ""
				},
				{
					"attributes", asset?.Metadata?.Attributes ?? ""
				},
				{
					"rawAttributes", asset?.Metadata?.RawAttributes ?? ""
				},
			};
			var meta = new JObject
			{
				{
					"metadata", subMeta
				},
			};

			return meta.ToString();
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="asset"></param>
		/// <returns>The URL pointing to the profile image for this asset</returns>
		public static string GetProfileImageUrl(this Asset asset)
		{
			return asset?.Metadata?.Properties?["image"]?.ToString() ?? "";
		}
	}
}