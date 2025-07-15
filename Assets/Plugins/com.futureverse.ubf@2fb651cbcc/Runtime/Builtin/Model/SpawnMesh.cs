// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Resources;
using Futureverse.UBF.Runtime.Utils;
using GLTFast;
using Plugins.UBF.Runtime;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class SpawnMesh : SpawnModel
	{
		public SpawnMesh(Context context) : base(context) { }

		protected override IEnumerator ExecuteAsync()
		{
			if (!TryReadResourceId("Resource", out var resourceId) || !resourceId.IsValid)
			{
				UbfLogger.LogError("[SpawnMesh] Could not find input \"Resource\"");
				yield break;
			}

			if (!TryRead<Transform>("Parent", out var parent))
			{
				UbfLogger.LogError("[SpawnMesh] Could not find input \"Parent\"");
				yield break;
			}

			if (!TryRead<RuntimeMeshConfig>("Config", out var runtimeConfig))
			{
				UbfLogger.LogWarn("[SpawnMesh] Failed to get input \"Config\"");
			}

			GltfImport gltfResource = null;
			MeshAssetImportSettings importSettings = null;
			var routine = CoroutineHost.Instance.StartCoroutine(
				NodeContext.ExecutionContext.Config.GetMeshInstance(
					resourceId,
					(resource, settings) =>
					{
						gltfResource = resource;
						importSettings = settings;
					}
				)
			);
			if (routine != null)
			{
				yield return routine;
			}

			if (gltfResource == null)
			{
				UbfLogger.LogError($"[SpawnMesh] Could not load Mesh resource with Id \"{resourceId.Value}\"");
				yield break;
			}

			// Standard v0.3.0 changed Resource<Mesh> to represent a single Mesh, rather than a whole GLB. It also
			// introduced Resource metadata that dictates which Mesh to use.
			GameObjectInstantiator instantiator;
			if (NodeContext.ExecutionContext.BlueprintVersionIsGreaterOrEqualTo("0.3.0"))
			{
				var validMeshNames = importSettings == null ?
					null :
					new List<string>
					{
						importSettings.LODMeshIdentifier,
					};
				instantiator = new UbfMeshInstantiator(gltfResource, parent, validMeshNames);
			}
			else
			{
				instantiator = new GameObjectInstantiator(gltfResource, parent);
			}

			instantiator.MeshAdded += MeshAddedCallback;
			var instantiateRoutine = CoroutineHost.Instance.StartCoroutine(
				new WaitForTask(gltfResource.InstantiateMainSceneAsync(instantiator))
			);
			if (instantiateRoutine != null)
			{
				yield return instantiateRoutine;
			}
			
			var glbReference = parent.gameObject.AddComponent<GLBReference>();
			glbReference.GLTFImport = gltfResource;
			var animator = parent.gameObject.GetComponentInParent<Animator>(includeInactive: true);
			
			// Extra yield here as we can't be sure that the mesh will be instantiated fully after the above task finishes
			yield return null;
			
			ApplyRuntimeConfig(runtimeConfig, animator);
			
			WriteOutput("Renderers", Renderers);
			WriteOutput("Scene Nodes", Transforms);
		}
	}
}