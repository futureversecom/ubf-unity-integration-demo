// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using Newtonsoft.Json.Linq;

namespace Futureverse.UBF.Runtime.Resources
{
	/// <summary>
	/// Simple implementation of the IResourceData interface. The hash is blank, meaning this resource won't
	/// be cached.
	/// </summary>
	public class BasicResource : IResourceData
	{
		public string Hash => "";
		public string Uri { get; }
		public JObject ImportSettings => null;
		public string StandardVersion => "";
		public ResourceType Type { get; }

		public BasicResource(string uri, ResourceType resourceType = ResourceType.Unspecified)
		{
			Uri = uri;
			Type = resourceType;
		}
	}
}