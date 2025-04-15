// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

namespace Futureverse.UBF.Runtime.Resources
{
	/// <summary>
	/// Extend this to provide a custom caching mechanism for resource downloading.
	/// </summary>
	public interface ICache
	{
		/// <summary>
		/// Attempts to retrieve the saved bytes of a cached resource.
		/// </summary>
		/// <param name="resourceData">Preferably the hash of the resource is used to index the saved data.</param>
		/// <param name="bytes">Raw bytes. Null if the bytes could not be retrieved or the resource doesn't exist.</param>
		/// <returns>If the bytes were successfully retrieved from the cache.</returns>
		bool TryGetCachedBytes(IResourceData resourceData, out byte[] bytes);
		/// <summary>
		/// Caches the raw bytes of a resource
		/// </summary>
		/// <param name="resourceData">Preferably the hash of the resource is used to index the saved data.</param>
		/// <param name="bytes">The raw bytes to store in the cache.</param>
		void CacheBytes(IResourceData resourceData, byte[] bytes);
	}
}