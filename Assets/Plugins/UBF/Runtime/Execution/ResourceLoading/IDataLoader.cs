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
	/// <typeparam name="T">Type of the data being loaded.</typeparam>
	public interface IDataLoader<out T> where T : class
	{
		/// <summary>
		/// Coroutine that loads an object from raw byte data.
		/// </summary>
		/// <param name="bytes">The raw bytes to turn into the object.</param>
		/// <param name="onComplete">Callback that contains the loaded object.</param>
		/// <returns>IEnumerator for yielding on.</returns>
		IEnumerator LoadFromData(byte[] bytes, Action<T> onComplete);
	}

	/// <summary>
	/// IDataLoader implementation for loading a Unity Texture2D from byte data. Allows settings to be configured before loading.
	/// </summary>
	public class TextureLoader : IDataLoader<Texture2D>
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

		public IEnumerator LoadFromData(byte[] bytes, Action<Texture2D> onComplete)
		{
			var texture = new Texture2D(
				2,
				2,
				TextureFormat.ARGB32,
				false,
				!_useSrgb
			);
			texture.LoadImage(bytes);
			onComplete?.Invoke(texture);
			yield break;
		}
	}
	
	/// <summary>
	/// IDataLoader implementation for loading a Gltf Loader from byte data.
	/// </summary>
	public class GltfLoader : IDataLoader<GltfImport>
	{
		private readonly GltfImport _gltfImport;

		public GltfLoader()
		{
			var logger = new ConsoleLogger();
			var deferAgent = new UninterruptedDeferAgent();
			_gltfImport = new GltfImport(deferAgent: deferAgent, logger: logger);
		}
		
		public IEnumerator LoadFromData(byte[] bytes, Action<GltfImport> onComplete)
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
	/// IDataLoader implementation for loading a UBF Blueprint from byte data. Allows the Instance Id to be configured before loading.
	/// </summary>
	public class BlueprintLoader : IDataLoader<Blueprint>
	{
		private string _instanceId;

		/// <summary>
		/// Sets the instance Id of the resulting Blueprint, making it easier to reference.
		/// </summary>
		/// <param name="instanceId">Instance Id of the resulting Blueprint</param>
		public void SetInstanceId(string instanceId)
		{
			_instanceId = instanceId;
		}
		
		public IEnumerator LoadFromData(byte[] bytes, Action<Blueprint> onComplete)
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
	public class JsonLoader<T> : IDataLoader<T> where T : class
	{
		public IEnumerator LoadFromData(byte[] bytes, Action<T> onComplete)
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
}