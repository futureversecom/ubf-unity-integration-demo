// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Builtin;
using Futureverse.UBF.Runtime.Resources;
using GLTFast;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Execution
{
	public interface IExecutionConfig
	{
		/// <summary>
		/// All objects spawned by a UBF execution with this config will be parented to this transform.
		/// </summary>
		Transform GetRootTransform { get; }
		void RegisterRuntimeResource(ResourceData resourceData);
		/// <summary>
		/// If ResourceId is an Instance ID, tries to retrieve a preloaded Blueprint. Otherwise, downloads the Blueprint from a Catalog.
		/// </summary>
		/// <param name="id">Resource ID or Instance ID of the Blueprint to load.</param>
		/// <param name="callback">Callback containing the loaded runtime Blueprint.</param>
		/// <returns>IEnumerator to yield on</returns>
		IEnumerator GetBlueprintInstance(ResourceId id, Action<Blueprint, BlueprintAssetImportSettings> callback);
		/// <summary>
		/// Loads an instance of a GltfImport which can be used to instantiate a glTF mesh in the scene.
		/// </summary>
		/// <param name="id">Resource ID of the Mesh from the Blueprint.</param>
		/// <param name="callback">Callback containing the loaded GltfImport component.</param>
		/// <returns></returns>
		IEnumerator GetMeshInstance(ResourceId id, Action<GltfImport, MeshAssetImportSettings> callback);
		/// <summary>
		/// Loads a Texture2D resource.
		/// </summary>
		/// <param name="id">Resource ID of the Mesh from the Blueprint.</param>
		/// <param name="settings">Texture settings that are applied to the loaded Texture2D.</param>
		/// <param name="callback">Callback containing the loaded Texture2D.</param>
		/// <returns>IEnumerator to yield on</returns>
		IEnumerator GetTextureInstance(ResourceId id, TextureImportSettings settings, Action<Texture2D, TextureAssetImportSettings> callback);
	}
	
	public class ExecutionConfig : IExecutionConfig
	{
		public Transform GetRootTransform { get; }

		private readonly Dictionary<string, Blueprint> _loadedBlueprints;

		public void RegisterRuntimeResource(ResourceData resourceData)
		{
			ArtifactProvider.Instance.RegisterRuntimeResource(resourceData.Id, resourceData);
		}

		public IEnumerator GetBlueprintInstance(ResourceId id, Action<Blueprint, BlueprintAssetImportSettings> callback)
		{
			if (_loadedBlueprints.TryGetValue(id.Value, out var loadedGraph))
			{
				callback?.Invoke(loadedGraph, null);
				yield break;
			}

			// instance ID could also be a resource id - check for a graph resource and create instance from it
			var guid = Guid.NewGuid().ToString();
			yield return ArtifactProvider.Instance.GetBlueprintResource(id, guid,
				(graph, importSettings) =>
				{
					_loadedBlueprints.Add(guid, graph);
					callback?.Invoke(graph, importSettings);
				});
		}

		public IEnumerator GetMeshInstance(ResourceId id, Action<GltfImport, MeshAssetImportSettings> callback)
		{
			return ArtifactProvider.Instance.GetMeshResource(id, callback);
		}

		public IEnumerator GetTextureInstance(ResourceId id, TextureImportSettings settings, Action<Texture2D, TextureAssetImportSettings> callback)
		{
			return ArtifactProvider.Instance.GetTextureResource(id, settings, callback);
		}
		
		/// <param name="rootTransform">The transform to which all objects spawned from this UBF execution will be parented.</param>
		/// <param name="loadedBlueprints">A map of Instance IDs to loaded Blueprints.</param>
		public ExecutionConfig(
			Transform rootTransform,
			Dictionary<string, Blueprint> loadedBlueprints)
		{
			GetRootTransform = rootTransform;
			_loadedBlueprints = loadedBlueprints;
		}
	}
}