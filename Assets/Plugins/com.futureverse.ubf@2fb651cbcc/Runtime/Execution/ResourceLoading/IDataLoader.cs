// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Text;
using GLTFast;
using GLTFast.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Resources
{
	/// <summary>
	/// Provides a mechanism for turning raw byte data (from download or cache) into an object of a given type.
	/// </summary>
	/// <typeparam name="TResource">Type of the data being loaded.</typeparam>
	/// <typeparam name="TImportSettings">The import settings for the resource.</typeparam>
	public interface IDataLoader<out TResource, in TImportSettings> where TResource : class where TImportSettings : AAssetImportSettings<TResource>
	{
		/// <summary>
		/// Coroutine that loads an object from raw byte data.
		/// </summary>
		/// <param name="bytes">The raw bytes to turn into the object.</param>
		/// <param name="importSettings">Contains settings for how the resource should be loaded or altered.</param>
		/// <param name="onComplete">Callback that contains the loaded object.</param>
		/// <returns>IEnumerator for yielding on.</returns>
		IEnumerator LoadFromData(byte[] bytes, TImportSettings importSettings, Action<TResource> onComplete);
	}

	/// <summary>
	/// IDataLoader implementation for loading a Unity Texture2D from byte data. Allows settings to be configured before loading.
	/// </summary>
	public class TextureLoader : IDataLoader<Texture2D, TextureAssetImportSettings>
	{
		private bool _useSrgb;

		/// <summary>
		/// Set whether this texture should be in sRGB color space.
		/// </summary>
		/// <param name="useSrgb">Use linear color space?</param>
		public void SetSrgb(bool useSrgb)
		{
			_useSrgb = useSrgb;
		}

		public IEnumerator LoadFromData(byte[] bytes, TextureAssetImportSettings importSettings, Action<Texture2D> onComplete)
		{
			// Next major release we can get rid of the _useSrgb member bool, and just use the one from import settings
			var useSrgb = _useSrgb;
			if (importSettings != null)
			{
				useSrgb = importSettings.IsSrgb;
			}
			
			var texture = new Texture2D(
				2,
				2,
				TextureFormat.ARGB32,
				false,
				!useSrgb
			);
			
			texture.LoadImage(bytes);
			onComplete?.Invoke(texture);
			yield break;
		}
	}
	
	/// <summary>
	/// IDataLoader implementation for loading a Gltf Loader from byte data.
	/// </summary>
	public class GltfLoader : IDataLoader<GltfImport, MeshAssetImportSettings>
	{
		private readonly GltfImport _gltfImport;

		public GltfLoader()
		{
			var logger = new ConsoleLogger();
			var deferAgent = new UninterruptedDeferAgent();
			_gltfImport = new GltfImport(deferAgent: deferAgent, logger: logger);
		}
		
		public IEnumerator LoadFromData(byte[] bytes, MeshAssetImportSettings importSettings, Action<GltfImport> onComplete)
		{
			var task = _gltfImport.Load(bytes);
			while (!task.IsCompleted)
			{
				yield return null;
			}
			
			onComplete?.Invoke(_gltfImport);
		}
	}

	/// <summary>
	/// IDataLoader implementation for loading a UBF Blueprint from byte data. Allows the Instance ID to be configured before loading.
	/// </summary>
	public class BlueprintLoader : IDataLoader<Blueprint, BlueprintAssetImportSettings>
	{
		private string _instanceId;

		/// <summary>
		/// Sets the instance ID of the resulting Blueprint, making it easier to reference.
		/// </summary>
		/// <param name="instanceId">Instance ID of the resulting Blueprint</param>
		public void SetInstanceId(string instanceId)
		{
			_instanceId = instanceId;
		}
		
		public IEnumerator LoadFromData(byte[] bytes, BlueprintAssetImportSettings importSettings, Action<Blueprint> onComplete)
		{
			var jsonString = Encoding.UTF8.GetString(bytes);
			if (Blueprint.TryLoad(_instanceId, jsonString, out var graph))
			{
				onComplete?.Invoke(graph);
				yield break;
			}

			onComplete?.Invoke(null);
		}
	}

	/// <summary>
	/// IDataLoader implementation for loading a given json object from byte data. Byte data is loaded into a string
	/// and then deserialized into the given type.
	/// </summary>
	/// <typeparam name="T">The type of object to deserialize the Json to</typeparam>
	public class JsonLoader<T> : IDataLoader<T, EmptyImportSettings<T>> where T : class
	{
		public IEnumerator LoadFromData(byte[] bytes, EmptyImportSettings<T> importSettings, Action<T> onComplete)
		{
			try
			{
				var jsonString = Encoding.UTF8.GetString(bytes);
				var deserializedJson = JsonConvert.DeserializeObject<T>(jsonString);
				onComplete?.Invoke(deserializedJson);
			}
			catch (Exception)
			{
				onComplete?.Invoke(null);
			}

			yield break;
		}
	}
	
	/// <summary>
	/// IDataLoader implementation for loading a Catalog object from byte data. Importantly, this sets the Standard
	/// Version of each resource to the version of the catalog
	/// </summary>
	public class CatalogLoader : IDataLoader<Catalog, EmptyImportSettings<Catalog>>
	{
		public IEnumerator LoadFromData(
			byte[] bytes,
			EmptyImportSettings<Catalog> importSettings,
			Action<Catalog> onComplete)
		{
			try
			{
				var jsonString = Encoding.UTF8.GetString(bytes);
				var catalog = JsonConvert.DeserializeObject<Catalog>(jsonString);

				// Assign the standard version to each of the resources
				foreach (var resource in catalog.Entries)
				{
					resource.StandardVersion = catalog.Version;
				}
				
				onComplete?.Invoke(catalog);
			}
			catch (Exception)
			{
				onComplete?.Invoke(null);
			}

			yield break;
		}
	}
}