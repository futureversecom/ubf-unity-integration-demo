// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using Futureverse.UBF.Runtime.Utils;

namespace Futureverse.UBF.Runtime.Resources
{
	/// <summary>
	/// Composable class for flexible downloading of resources. Add custom caching, downloading, and loading behavior
	/// to get resources in any matter that is required.
	/// </summary>
	/// <typeparam name="TResource">The type of the resource to be loaded</typeparam>
	/// <typeparam name="TImportSettings">The import settings for the object</typeparam>
	public class ResourceLoader<TResource, TImportSettings> where TResource : class where TImportSettings : AAssetImportSettings<TResource>
	{
		private readonly ICache _cache;
		private readonly IDownloader _downloader;
		private readonly IDataLoader<TResource, TImportSettings> _dataLoader;
		private readonly IResourceData _resourceData;

		/// <summary>
		/// Creates a ResourceLoader with configurable components.
		/// </summary>
		/// <param name="resourceData">The resource to load.</param>
		/// <param name="downloader">Specifies how the resource data should be downloaded.</param>
		/// <param name="dataLoader">Specifies how the downloaded bytes should be converted into an object.</param>
		/// <param name="cache">Optional cache mechanism to reduce multiple downloads.</param>
		public ResourceLoader(
			IResourceData resourceData,
			IDownloader downloader,
			IDataLoader<TResource, TImportSettings> dataLoader,
			ICache cache = null)
		{
			_resourceData = resourceData;
			_cache = cache;
			_dataLoader = dataLoader;
			_downloader = downloader;
		}

		/// <summary>
		/// Loads the resource.
		/// </summary>
		/// <param name="onComplete">Callback containing the loaded resource and import settings for that resource.</param>
		/// <returns>IEnumerator to yield on.</returns>
		public IEnumerator Get(Action<TResource, TImportSettings> onComplete)
		{
			byte[] bytes = null;
			if (_cache != null && _cache.TryGetCachedBytes(_resourceData, out var cachedBytes))
			{
				bytes = cachedBytes;
			}
			else
			{
				var downloadRoutine = CoroutineHost.Instance.StartCoroutine(
					_downloader.DownloadBytes(
						_resourceData.Uri,
						resultBytes =>
						{
							bytes = resultBytes;
							if (resultBytes != null)
							{
								_cache?.CacheBytes(_resourceData, resultBytes);
							}
						}
					)
				);
				if (downloadRoutine != null)
				{
					yield return downloadRoutine;
				}
			}

			if (bytes == null)
			{
				onComplete?.Invoke(null, null);
				yield break;
			}

			// Propagate version from resource data to import settings
			var importSettings = _resourceData.ImportSettings?.ToObject<TImportSettings>();
			if (importSettings != null && Version.TryParse(_resourceData.StandardVersion, out var version))
			{
				importSettings.StandardVersion = version;
			}

			var loadRoutine = CoroutineHost.Instance.StartCoroutine(
				_dataLoader.LoadFromData(
					bytes,
					importSettings,
					(resource) =>
					{
						onComplete?.Invoke(resource, importSettings);
					}
				)
			);
			if (loadRoutine != null)
			{
				yield return loadRoutine;
			}
		}
		
		/// <summary>
		/// Same as original Get method, but discards the import settings parameter for simplicity.
		/// </summary>
		/// <param name="onComplete">Callback containing the loaded resource.</param>
		/// <returns>IEnumerator to yield on.</returns>
		public IEnumerator Get(Action<TResource> onComplete)
		{
			var routine = CoroutineHost.Instance.StartCoroutine(Get((resource, _) => onComplete(resource)));
			if (routine != null)
			{
				yield return routine;
			}
		}

		/// <summary>
		/// Only downloads the bytes and stores them in the cache. Does nothing if the provided cache was null.
		/// </summary>
		/// <returns>IEnumerator to yield on.</returns>
		public IEnumerator Preload()
		{
			if (_cache == null)
			{
				UbfLogger.LogInfo("Skipping preload - no cache assigned");
				yield break;
			}

			if (_cache.TryGetCachedBytes(_resourceData, out _))
			{
				UbfLogger.LogInfo("Skipping preload - already loaded");
				yield break;
			}

			var downloadRoutine = CoroutineHost.Instance.StartCoroutine(
				_downloader.DownloadBytes(
					_resourceData.Uri,
					resultBytes =>
					{
						if (resultBytes != null)
						{
							_cache.CacheBytes(_resourceData, resultBytes);
						}
					}
				)
			);

			if (downloadRoutine != null)
			{
				yield return downloadRoutine;
			}
		}
	}

	public class JsonResourceLoader<T> : ResourceLoader<T, EmptyImportSettings<T>> where T : class
	{
		public JsonResourceLoader(string uri) : base(
			new BasicResource(uri),
			new DefaultDownloader(),
			new JsonLoader<T>()
		)
		{
			
		}
	}
	
	public class CatalogResourceLoader : ResourceLoader<Catalog, EmptyImportSettings<Catalog>>
	{
		public CatalogResourceLoader(string uri) : base(
			new BasicResource(uri),
			new DefaultDownloader(),
			new CatalogLoader()
		)
		{
			
		}
	}
}