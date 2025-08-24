// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
			if (!TryReadResourceId("Mesh", out var resourceId) || !resourceId.IsValid)
			{
				UbfLogger.LogError("[SpawnMesh] Could not find input \"Mesh\"");
				yield break;
			}

			if (!TryRead<SceneNode>("Parent", out var parent))
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
			var settings = new InstantiationSettings()
			{
				SceneObjectCreation = SceneObjectCreation.Always
			};
			if (NodeContext.ExecutionContext.BlueprintVersionIsGreaterOrEqualTo("0.3.0"))
			{
				var validMeshNames = importSettings == null ?
					null :
					new List<string>
					{
						importSettings.LODMeshIdentifier,
					};
				instantiator = new UbfMeshInstantiator(gltfResource, parent.TargetSceneObject.transform, validMeshNames, settings: settings);
			}
			else
			{
				instantiator = new GameObjectInstantiator(gltfResource, parent.TargetSceneObject.transform, settings: settings);
			}

			instantiator.MeshAdded += MeshAddedCallback;
			var instantiateRoutine = CoroutineHost.Instance.StartCoroutine(
				new WaitForTask(gltfResource.InstantiateMainSceneAsync(instantiator))
			);
			if (instantiateRoutine != null)
			{
				yield return instantiateRoutine;
			}

			var root = instantiator.SceneTransform;
			var rootNode = new SceneNode()
            {
            	TargetSceneObject = root.gameObject
            };
			
			rootNode.AddComponents(Renderers); // Should only be a single element, but nothing wrong with doing a foreach JIC

            if (Renderers.Count > 0)
            {
	            rootNode.Name = Renderers[0].TargetMeshRenderers[0].gameObject.name;
            }
            
            if (Renderers.Any(x => x.skinned))
            {
	            var skR = Renderers.First(x => x.skinned);
	            //var rigRoot = (skR.TargetMeshRenderers[0] as SkinnedMeshRenderer).rootBone;
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

			var glbReference = parent.TargetSceneObject.AddComponent<GLBReference>();
			glbReference.GLTFImport = gltfResource;
			//var animator = parent.TargetSceneObject.GetComponentInParent<Animator>(includeInactive: true);
			
			// Extra yield here as we can't be sure that the mesh will be instantiated fully after the above task finishes
			yield return null;
			
			ApplyRuntimeConfig(runtimeConfig);
			
			WriteOutput("Renderer", Renderers.FirstOrDefault());
			WriteOutput("SceneNode", rootNode);
		}
	}
}