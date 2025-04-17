// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Utils;
using GLTFast;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class SpawnMesh : ACustomNode
	{
		public SpawnMesh(Context context) : base(context) { }

		protected override IEnumerator ExecuteAsync()
		{
			if (!TryReadResourceId("Resource", out var resourceId) || !resourceId.IsValid)
			{
				Debug.LogError("[SpawnMesh] Failed to find resource \"Resource\"");
				TriggerNext();
				yield break;
			}

			if (!TryRead<Transform>("Parent", out var parent))
			{
				Debug.LogError("[SpawnMesh] Failed to get input 'Parent'");
				TriggerNext();
				yield break;
			}

			if (!TryRead<RuntimeMeshConfig>("Config", out var runtimeConfig))
			{
				Debug.Log("[SpawnMesh] Failed to get input 'Config'");
			}

			GltfImport gltfResource = null;
			var routine = CoroutineHost.Instance.StartCoroutine(
				NodeContext.ExecutionContext.Config.GetMeshInstance(resourceId, resource => { gltfResource = resource; })
			);
			if (routine != null)
			{
				yield return routine;
			}

			if (gltfResource == null)
			{
				Debug.LogError($"[SpawnMesh] Failed to get gltf resource with Id {resourceId.Value}");
				TriggerNext();
				yield break;
			}

			var glbReference = parent.gameObject.AddComponent<GLBReference>();
			glbReference.GLTFImport = gltfResource;

			var instantiator = new GameObjectInstantiator(gltfResource, parent);
			var renderersArray = Dynamic.Array();
			var sceneNodesArray = Dynamic.Array();
			List<SkinnedMeshRenderer> skinnedMeshes = new();
			instantiator.MeshAdded += (
				gameObject,
				_,
				_,
				_,
				_,
				_,
				_,
				_) =>
			{
				sceneNodesArray.Push(Dynamic.From(gameObject.transform));
				var renderer = gameObject.GetComponent<Renderer>();
				if (renderer != null)
				{
					renderersArray.Push(Dynamic.From(renderer));
					if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
					{
						skinnedMeshes.Add(skinnedMeshRenderer);
					}
				}
			};

			var instantiateRoutine = CoroutineHost.Instance.StartCoroutine(
				new WaitForTask(gltfResource.InstantiateMainSceneAsync(instantiator))
			);
			if (instantiateRoutine != null)
			{
				yield return instantiateRoutine;
			}

			yield return null;
			yield return null;

			if (runtimeConfig != null)
			{
				foreach (var renderer in skinnedMeshes)
				{
					Debug.Log($"Retargeting {renderer.name} with spawned config {runtimeConfig.Config.name}");
					RigUtilities.RetargetRig(runtimeConfig.RuntimeObject.transform, renderer);
				}
			}
			WriteOutput("Renderers", renderersArray);
			WriteOutput("Scene Nodes", sceneNodesArray);
			TriggerNext();
		}
	}
}