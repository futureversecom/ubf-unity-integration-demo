// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.IO;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Resources
{
	/// <summary>
	/// ICache implementation that stores cached bytes on disk on the user's machine. Useful for storing resources
	/// between sessions.
	/// </summary>
	public class ReadThroughCache : ICache
	{
		private readonly string _baseCachePath;
		
		/// <param name="baseCachePath">The path on the user's machine to store the data</param>
		public ReadThroughCache(string baseCachePath)
		{
			_baseCachePath = baseCachePath;
		}

		public void CacheBytes(IResourceData resourceData, byte[] bytes)
		{
			if (string.IsNullOrEmpty(resourceData.Hash))
			{
				return;
			}

			var cachePath = Path.Combine(_baseCachePath, resourceData.Hash);
			var directoryName = Path.GetDirectoryName(cachePath);

			if (Directory.Exists(directoryName))
			{
				File.WriteAllBytes(cachePath, bytes);
			}
			else if (!string.IsNullOrEmpty(directoryName))
			{
				Directory.CreateDirectory(directoryName);
				File.WriteAllBytes(cachePath, bytes);
			}
		}

		public bool TryGetCachedBytes(IResourceData resourceData, out byte[] bytes)
		{
			if (string.IsNullOrEmpty(resourceData.Hash))
			{
				bytes = null;
				return false;
			}

			var cachePath = Path.Combine(_baseCachePath, resourceData.Hash);
			if (!File.Exists(cachePath))
			{
				UbfLogger.LogInfo($"Cache miss for {resourceData.Uri}");
				bytes = null;
				return false;
			}

			bytes = File.ReadAllBytes(cachePath);
			return true;
		}
	}
}