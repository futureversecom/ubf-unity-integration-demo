// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;

namespace Futureverse.UBF.Runtime.Resources
{
	/// <summary>
	/// ICache implementation that stores the loaded bytes in memory. Only persists on this object.
	/// </summary>
	public class InMemoryCache : ICache
	{
		private readonly Dictionary<string, byte[]> _cachedBytes = new();

		public void CacheBytes(IResourceData resourceData, byte[] bytes)
		{
			_cachedBytes.TryAdd(resourceData.Uri, bytes);
		}

		public bool TryGetCachedBytes(IResourceData resourceData, out byte[] bytes)
		{
			if (_cachedBytes.TryGetValue(resourceData.Uri, out var cachedBytes))
			{
				bytes = cachedBytes;
				return true;
			}

			bytes = null;
			return false;
		}
	}
}