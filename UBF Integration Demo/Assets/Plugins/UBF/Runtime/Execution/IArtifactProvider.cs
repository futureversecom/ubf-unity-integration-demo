// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Builtin;
using Futureverse.UBF.Runtime.Resources;
using Futureverse.UBF.Runtime.Utils;
using GLTFast;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Execution
{
	/// <summary>
	/// Provides a way to download resources for a given set of Blueprints, via that Blueprint's Resource Ids. 
	/// </summary>
	public interface IArtifactProvider
	{
		/// <param name="resourceId">Resource ID from the Blueprint.</param>
		/// <param name="settings">Options to set on the loaded Texture.</param>
		/// <param name="onComplete">Callback containing the loaded Texture.</param>
		/// <returns></returns>
		IEnumerator GetTextureResource(
			ResourceId resourceId,
			TextureImportSettings settings,
			Action<Texture2D> onComplete);
		/// <param name="resourceId">Resource ID from the Blueprint.</param>
		/// <param name="instanceId">Options to set on the loaded Texture.</param>
		/// <param name="onComplete">Callback containing the loaded Blueprint.</param>
		/// <returns></returns>
		IEnumerator GetBlueprintResource(ResourceId resourceId, string instanceId, Action<Blueprint> onComplete);
		/// <param name="resourceId">Resource ID from the Blueprint.</param>
		/// <param name="onComplete">Callback containing the loaded GltfImport component.</param>
		/// <returns></returns>
		IEnumerator GetMeshResource(ResourceId resourceId, Action<GltfImport> onComplete);
	}

	/// <summary>
	/// ArtifactProvider that uses ResourceLoaders and optional caching to download and manage Blueprint resources.
	/// </summary>
	public class ArtifactProvider : IArtifactProvider
	{
		private readonly Dictionary<string, IResourceData> _catalog = new();
		private readonly ICache _resourceCache;

		public ArtifactProvider()
		{
			_resourceCache = null;
		}
		
		/// <param name="cache">The cache system you want to use for this provider.</param>
		public ArtifactProvider(ICache cache)
		{
			_resourceCache = cache;
		}

		/// <summary>
		/// Adds the catalog entries to this ArtifactProvider's list of resources.
		/// </summary>
		/// <param name="data">The catalog data describing which resources can be loaded.</param>
		public void RegisterCatalog(Catalog data)
		{
			foreach (var entry in data.Entries)
			{
				_catalog.TryAdd(entry.Id, entry);
			}
		}

		public IEnumerator GetTextureResource(
			ResourceId resourceId,
			TextureImportSettings settings,
			Action<Texture2D> onComplete)
		{
			var loader = new TextureLoader();
			loader.SetSrgb(settings.UseSrgb);

			var routine = CoroutineHost.Instance.StartCoroutine(
				GetResource<Texture2D, TextureLoader, DefaultDownloader>(resourceId, onComplete, loader)
			);
			if (routine != null)
			{
				yield return routine;
			}
		}

		public IEnumerator GetBlueprintResource(ResourceId resourceId, string instanceId, Action<Blueprint> onComplete)
		{
			var loader = new BlueprintLoader();
			loader.SetInstanceId(instanceId);
			
			var routine = CoroutineHost.Instance.StartCoroutine(
				GetResource<Blueprint, BlueprintLoader, DefaultDownloader>(resourceId, onComplete, loader)
			);
			if (routine != null)
			{
				yield return routine;
			}
		}

		public IEnumerator GetMeshResource(ResourceId resourceId, Action<GltfImport> onComplete)
		{
			var routine = CoroutineHost.Instance.StartCoroutine(
				GetResource<GltfImport, GltfLoader, DefaultDownloader>(resourceId, onComplete)
			);
			if (routine != null)
			{
				yield return routine;
			}
		}

		private IEnumerator GetResource<TResource, TLoader, TDownloader>(
			ResourceId resourceId,
			Action<TResource> onComplete,
			TLoader loader = null,
			TDownloader downloader = null)
			where TResource : class
			where TLoader : class, IDataLoader<TResource>, new()
			where TDownloader : class, IDownloader, new()
		{
			if (!_catalog.TryGetValue(resourceId.Value, out var resource))
			{
				onComplete?.Invoke(null);
				yield break;
			}

			var resourceLoader = new ResourceLoader<TResource>(
				resource,
				downloader ?? new TDownloader(),
				loader ?? new TLoader(),
				_resourceCache
			);

			var routine = CoroutineHost.Instance.StartCoroutine(resourceLoader.Get(onComplete));
			if (routine != null)
			{
				yield return routine;
			}
		}
	}
}