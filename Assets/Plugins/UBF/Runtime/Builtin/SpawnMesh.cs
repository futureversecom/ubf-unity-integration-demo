// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Utils;
using GLTFast;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class SpawnMesh : ACustomExecNode
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
                UbfLogger.LogError("[SpawnMesh] Failed to get input 'Config'");
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
				UbfLogger.LogError($"[SpawnMesh] Could not load GLB resource with Id {resourceId.Value}");
				yield break;
			}

			var glbReference = parent.gameObject.AddComponent<GLBReference>();
			glbReference.GLTFImport = gltfResource;

			var instantiator = new GameObjectInstantiator(gltfResource, parent);
			var renderersArray = new List<Renderer>();
			var sceneNodesArray = new List<Transform>();
			//List<SkinnedMeshRenderer> skinnedMeshes = new();
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
				sceneNodesArray.Add(gameObject.transform);
				var renderer = gameObject.GetComponent<Renderer>();
				if (renderer != null)
				{
					renderersArray.Add(renderer);
					//if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
					//{
						//skinnedMeshes.Add(skinnedMeshRenderer);
					//}
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

			var skinnedMeshes = renderersArray.Where(x => x is SkinnedMeshRenderer).Cast<SkinnedMeshRenderer>().ToArray();
			if (runtimeConfig != null && runtimeConfig.RuntimeObject != null && skinnedMeshes.Length > 0)
			{
			 	foreach (var renderer in skinnedMeshes)
			 	{
			 		Debug.Log($"Retargeting {renderer.name} with spawned config {runtimeConfig.Config.name}");
			 		RigUtilities.RetargetRig(runtimeConfig.RuntimeObject.transform, renderer);
			 	}
			    
			    var animator = glbReference.GetComponentInParent<Animator>(includeInactive: true);
			    if (animator != null && runtimeConfig.Config.avatar != null)
			    {
				    animator.avatar = runtimeConfig.Config.avatar;
			    }
			}
			WriteOutput("Renderers", renderersArray);
			WriteOutput("Scene Nodes", sceneNodesArray);
		}
	}
}