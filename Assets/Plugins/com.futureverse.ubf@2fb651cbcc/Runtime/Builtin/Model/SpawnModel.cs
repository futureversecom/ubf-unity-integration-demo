// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Utils;
using GLTFast;
using Plugins.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class SpawnModel : ACustomExecNode
	{
		protected readonly List<MeshRendererSceneComponent> Renderers = new();
		protected readonly List<MeshRendererSceneComponent> SkinnedMeshRenderers = new();
		
		public SpawnModel(Context context) : base(context) { }

		protected override IEnumerator ExecuteAsync()
		{
			if (!TryReadResourceId("Resource", out var resourceId) || !resourceId.IsValid)
			{
				UbfLogger.LogError("[SpawnModel] Could not find input \"Resource\"");
				yield break;
			}

			if (!TryRead<SceneNode>("Parent", out var parent))
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

			var settings = new InstantiationSettings()
			{
				SceneObjectCreation = SceneObjectCreation.Always
			};
			var instantiator = new GameObjectInstantiator(gltfResource, parent.TargetSceneObject.transform, settings: settings);
			
			instantiator.MeshAdded += MeshAddedCallback;

			var instantiateRoutine = CoroutineHost.Instance.StartCoroutine(
				new WaitForTask(gltfResource.InstantiateMainSceneAsync(instantiator))
			);
			if (instantiateRoutine != null)
			{
				yield return instantiateRoutine;
			}

			// Get root spawned node
			var root = instantiator.SceneTransform; 
			var rootNode = new SceneNode()
			{
				TargetSceneObject = root.gameObject
			};

			rootNode.AddComponents(Renderers);

			if (Renderers.Count > 0)
			{
				rootNode.Name = Renderers[0].TargetMeshRenderers[0].gameObject.name;
			}
			
			// If theres a rig, register it
			if (Renderers.Any(x => x.skinned))
			{
				var skR = Renderers.First(x => x.skinned);
				//var rigRoot = (skR.TargetMeshRenderers[0] as SkinnedMeshRenderer);
				var rig = RigSceneComponent.CreateFromSMR(skR.TargetMeshRenderers[0] as SkinnedMeshRenderer);
				/*
				var rigRootNode = SceneNode.BuildSceneTree(rigRoot, out var boneNodes);
				var rig = new RigSceneComponent
				{
					Node = rootNode,
					Bones = boneNodes,
					Root = rigRootNode
				};
				*/

				rootNode.AddComponent(rig);
			}
			
			parent.AddChild(rootNode);
			
			var glbReference = parent.TargetSceneObject.AddComponent<GLBReference>(); // Maybe place this on the instantiator scene transform? For later
			glbReference.GLTFImport = gltfResource;
			
			// Extra yield here as we can't be sure that the mesh will be instantiated fully after the above task finishes
			yield return null;
			
			ApplyRuntimeConfig(runtimeConfig);
			
			WriteOutput("Renderers", Renderers);
			WriteOutput("Scene Node", rootNode);
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
			var renderer = gameObject.GetComponent<Renderer>();
			if (renderer == null)
			{
				return;
			}

			var renderComponent = new MeshRendererSceneComponent()
			{
				TargetMeshRenderers = new List<Renderer>() { renderer },
				skinned = (renderer is SkinnedMeshRenderer)
			};
			Renderers.Add(renderComponent);
			if (renderComponent.skinned)
			{
				SkinnedMeshRenderers.Add(renderComponent);
			}
		}

		protected void ApplyRuntimeConfig(RuntimeMeshConfig runtimeConfig)
		{
			if (runtimeConfig == null || runtimeConfig.AnimationObject == null || SkinnedMeshRenderers.Count <= 0)
			{
				return;
			}
			var runtimeSMR = runtimeConfig.AnimationObject.GetComponentInChildren<SkinnedMeshRenderer>();

			foreach (var renderComponents in SkinnedMeshRenderers)
			{
				foreach (var mRender in renderComponents.TargetMeshRenderers)
				{
					UbfLogger.LogInfo(
						$"[{GetType().Name}] Retargeting \"{mRender.name}\" with spawned config \"{runtimeConfig.Config.name}\""
					);
					if (runtimeSMR != null)
					{
						RigUtils.RetargetRig(runtimeSMR, mRender as SkinnedMeshRenderer); // Assume if it lives in SkinnedMeshRenderers that it fits the type
					}
					else
					{
						RigUtils.RetargetRig(runtimeConfig.AnimationObject.transform, mRender as SkinnedMeshRenderer);
					}
				}
			}
		}
	}
}