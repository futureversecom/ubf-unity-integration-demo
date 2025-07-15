// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Futureverse.UBF.Runtime.Resources
{
	/// <summary>
	/// Represents a UBF Standard Catalog. Can be created manually or deserialized from Json.
	/// </summary>
	[JsonObject]
	public class Catalog
	{
		[JsonProperty(PropertyName = "version")]
		public string Version;
		[JsonProperty(PropertyName = "resources")]
		public List<ResourceData> Entries = new();

		/// <summary>
		/// Register a custom resource to the Catalog.
		/// </summary>
		/// <param name="resource">Data for a Catalog resource.</param>
		public void AddResource(ResourceData resource)
		{
			Entries.Add(resource);
		}
	}

	/// <summary>
	/// Can be created manually via the constructor, or deserializied from Json.
	/// </summary>
	[JsonObject]
	public class ResourceData : IResourceData
	{
		[JsonProperty("id")] private string _resourceId;
		[JsonProperty("uri")] private string _uri;
		[JsonProperty("type")] private ResourceType _type;
		[JsonProperty("hash")] private string _hash;
		[JsonProperty("metadata")] private JObject _importSettings;

		public string StandardVersion { get; set; }
		
		public string Id => _resourceId;
		public string Uri => _uri;
		public ResourceType Type => _type;
		public string Hash => _hash;
		public JObject ImportSettings => _importSettings;

		[JsonConstructor]
		private ResourceData()
		{
			
		}

		/// <param name="resourceId">Used to index the resource. Should match the resource ID from the target Blueprint.</param>
		/// <param name="uri">URL or local path that points to the resource.</param>
		/// <param name="type"></param>
		/// <param name="importSettings"></param>
		public ResourceData(string resourceId, string uri, ResourceType type = ResourceType.Unspecified, JObject importSettings = null)
		{
			_resourceId = resourceId;
			_uri = uri;
			_type = type;
			_importSettings = importSettings;
		}
	}
}