// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Builtin;
using Futureverse.UBF.Runtime.Resources;
using Futureverse.UBF.Runtime.Utils;
using GLTFast;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Execution
{
	/// <summary>
	/// ArtifactProvider that uses ResourceLoaders and optional caching to download and manage Blueprint resources.
	/// </summary>
	public class ArtifactProvider
	{
		public static ArtifactProvider Instance => s_instance ??= new ArtifactProvider();
		
		private readonly Dictionary<string, IResourceData> _catalog = new();
		private readonly List<IDownloader> _registeredDownloaders = new();
		private static ArtifactProvider s_instance;
		private ICache _resourceCache;

		private ArtifactProvider()
		{
			
		}
		
		public void SetCache(ICache cache)
		{
			_resourceCache = cache;
		}
		
		public void RegisterDownloader(IDownloader downloader)
		{
			_registeredDownloaders.Add(downloader);
		}

		/// <summary>
		/// Adds the catalog entries to this ArtifactProvider's list of resources.
		/// </summary>
		/// <param name="data">The catalog data describing which resources can be loaded.</param>
		public void RegisterCatalog(Catalog data)
		{
			foreach (var entry in data.Entries)
			{
				RegisterRuntimeResource(entry.Id, entry);
			}
		}

		public void RegisterRuntimeResource(string resourceId, IResourceData resource)
		{
			_catalog.TryAdd(resourceId, resource);
		}

		public IEnumerator GetTextureResource(
			ResourceId resourceId,
			TextureImportSettings settings,
			Action<Texture2D, TextureAssetImportSettings> onComplete)
		{
			var loader = new TextureLoader();
			if (settings != null)
			{
				loader.SetSrgb(settings.UseSrgb);
			}

			return GetResource(
				resourceId,
				ResourceType.Texture,
				onComplete,
				loader
			);
		}

		public IEnumerator GetBlueprintResource(ResourceId resourceId, string instanceId, Action<Blueprint, BlueprintAssetImportSettings> onComplete)
		{
			var loader = new BlueprintLoader();
			loader.SetInstanceId(instanceId);

			return GetResource(
				resourceId,
				ResourceType.Blueprint,
				onComplete,
				loader
			);
		}

		public IEnumerator GetMeshResource(ResourceId resourceId, Action<GltfImport, MeshAssetImportSettings> onComplete)
		{
			return GetResource<GltfImport, GltfLoader, MeshAssetImportSettings>(
				resourceId,
				ResourceType.Mesh,
				onComplete
			);
		}

		protected IEnumerator GetResource<TResource, TLoader, TImportSettings>(
			ResourceId resourceId,
			ResourceType type,
			Action<TResource, TImportSettings> onComplete,
			TLoader loader = null)
			where TResource : class
			where TLoader : class, IDataLoader<TResource, TImportSettings>, new()
			where TImportSettings : AAssetImportSettings<TResource>
		{
			if (!_catalog.TryGetValue(resourceId.Value, out var resource))
			{
				UbfLogger.LogWarn($"No resource found with Id \"{resourceId.Value}\"");
				onComplete?.Invoke(null, null);
				yield break;
			}

			var downloader = GetDownloader(resource);
			if (downloader == null)
			{
				UbfLogger.LogWarn($"Can't get downloader for resource with Id \"{resourceId.Value}\"");
				onComplete?.Invoke(null, null);
				yield break;
			}

			var resourceLoader = new ResourceLoader<TResource, TImportSettings>(
				resource,
				downloader,
				loader ?? new TLoader(),
				_resourceCache
			);
			
			var routine = CoroutineHost.Instance.StartCoroutine(resourceLoader.Get(onComplete));
			if (routine != null)
			{
				yield return routine;
			}
		}

		private IDownloader GetDownloader(IResourceData resource)
			=> _registeredDownloaders.FirstOrDefault(d => d.CanDownload(resource)) ?? new DefaultDownloader();
	}
}