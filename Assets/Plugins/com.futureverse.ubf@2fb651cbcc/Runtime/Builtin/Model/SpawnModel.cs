// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Utils;
using GLTFast;
using Plugins.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class SpawnModel : ACustomExecNode
	{
		protected readonly List<Renderer> Renderers = new();
		protected readonly List<Transform> Transforms = new();
		protected readonly List<SkinnedMeshRenderer> SkinnedMeshRenderers = new();
		
		public SpawnModel(Context context) : base(context) { }

		protected override IEnumerator ExecuteAsync()
		{
			if (!TryReadResourceId("Resource", out var resourceId) || !resourceId.IsValid)
			{
				UbfLogger.LogError("[SpawnModel] Could not find input \"Resource\"");
				yield break;
			}

			if (!TryRead<Transform>("Parent", out var parent))
			{
				UbfLogger.LogError("[SpawnModel] Could not find input \"Parent\"");
				yield break;
			}

			if (!TryRead<RuntimeMeshConfig>("Config", out var runtimeConfig))
			{
				UbfLogger.LogWarn("[SpawnModel] Failed to get input \"Config\"");
			}

			GltfImport gltfResource = null;
			var routine = CoroutineHost.Instance.StartCoroutine(
				NodeContext.ExecutionContext.Config.GetMeshInstance(
					resourceId,
					(resource, _) =>
					{
						gltfResource = resource;
					}
				)
			);
			if (routine != null)
			{
				yield return routine;
			}

			if (gltfResource == null)
			{
				UbfLogger.LogError($"[SpawnModel] Could not load GLB resource with Id \"{resourceId.Value}\"");
				yield break;
			}
			
			var instantiator = new GameObjectInstantiator(gltfResource, parent);
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
		
		protected virtual void MeshAddedCallback(
			GameObject gameObject,
			uint nodeIndex,
			string meshName,
			MeshResult meshResult,
			uint[] joints,
			uint? rootJoint,
			float[] morphTargetWeights,
			int meshNumeration)
		{
			Transforms.Add(gameObject.transform);
			var renderer = gameObject.GetComponent<Renderer>();
			if (renderer == null)
			{
				return;
			}

			Renderers.Add(renderer);
			if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
			{
				SkinnedMeshRenderers.Add(skinnedMeshRenderer);
			}
		}

		protected void ApplyRuntimeConfig(RuntimeMeshConfig runtimeConfig, Animator animator)
		{
			if (runtimeConfig == null || runtimeConfig.RuntimeObject == null || SkinnedMeshRenderers.Count <= 0)
			{
				return;
			}

			foreach (var renderer in SkinnedMeshRenderers)
			{
				UbfLogger.LogInfo(
					$"[{GetType().Name}] Retargeting \"{renderer.name}\" with spawned config \"{runtimeConfig.Config.name}\""
				);
				RigUtils.RetargetRig(runtimeConfig.RuntimeObject.transform, renderer);
			}
				
			if (animator != null && runtimeConfig.Config.Avatar != null)
			{
				animator.avatar = runtimeConfig.Config.Avatar;
			}
		}
	}
}