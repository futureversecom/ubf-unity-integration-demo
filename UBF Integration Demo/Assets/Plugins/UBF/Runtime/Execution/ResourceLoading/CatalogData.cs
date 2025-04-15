// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Futureverse.UBF.Runtime.Resources
{
	/// <summary>
	/// Represents a UBF Standard Catalog. Can be created manually or deserialized from Json.
	/// </summary>
	[JsonObject]
	public class Catalog
	{
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
		[JsonProperty("hash")] private string _hash;

		public string Id => _resourceId;
		public string Uri => _uri;
		public string Hash => _hash;

		private ResourceData()
		{
			
		}
		
		/// <param name="resourceId">Used to index the resource. Should match the resource Id from the target Blueprint.</param>
		/// <param name="uri">URL or local path that points to the resource.</param>
		public ResourceData(string resourceId, string uri)
		{
			_resourceId = resourceId;
			_uri = uri;
		}
	}
}