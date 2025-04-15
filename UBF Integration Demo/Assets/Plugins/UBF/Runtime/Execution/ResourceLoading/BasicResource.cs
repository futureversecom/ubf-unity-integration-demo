// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

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

		public BasicResource(string uri)
		{
			Uri = uri;
		}
	}
}