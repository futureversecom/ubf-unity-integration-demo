// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Resources;
using Futureverse.UBF.Runtime.Settings;
using Futureverse.UBF.Runtime.Utils;
using GLTFast;
using Plugins.UBF.Runtime;
using Plugins.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class SpawnModelWithLods : SpawnModel
	{
		private class LodData
		{
			public GltfImport Mesh;
			public List<string> MeshNames;
		}
		
		public SpawnModelWithLods(Context context) : base(context) { }

		private readonly List<LodData> _lodData = new();
		private LODGroup _lodGroup;
		private int _numLods;
		private int _currentLodNum;
		private LOD[] _lods;
		
		protected override IEnumerator ExecuteAsync()
		{
			if (!TryReadArray<ResourceId>("Meshes", out var resources))
			{
				UbfLogger.LogError("[SpawnModelWithLODs] Could not find input \"Meshes\"");
				yield break;
			}

			if (resources.Count < 1)
			{
				UbfLogger.LogError("[SpawnModelWithLODs] Number of meshes is 0, cannot spawn.");
				yield break;
			}

			if (!TryRead<SceneNode>("Parent", out var parent))
			{
				UbfLogger.LogError("[SpawnModelWithLODs] Could not find input \"Parent\"");
				yield break;
			}

			if (!TryRead<RuntimeMeshConfig>("Config", out var runtimeConfig))
			{
				UbfLogger.LogWarn("[SpawnModelWithLODs] Failed to get input \"Config\"");
			}
			
			var lodParent = new GameObject("LODGroup");
			lodParent.transform.SetParent(parent.TargetSceneObject.transform);
			_lodGroup = lodParent.AddComponent<LODGroup>();
			
			_numLods = resources.Count;
			_lods = new LOD[_numLods];

			var rootNode = new SceneNode()
			{
				TargetSceneObject = lodParent
			};
			
			foreach (var resource in resources)
			{
				yield return LoadLodResource(resource);
			}

			foreach (var lodData in _lodData)
			{
				yield return SpawnLodMesh(lodData, lodParent.transform);
			}
			
			_lodGroup.SetLODs(_lods);
			var animator = parent.TargetSceneObject.GetComponentInParent<Animator>(includeInactive: true);
			
			// Extra yield here as we can't be sure that the mesh will be instantiated fully after the above task finishes
			yield return null;
			
			if (Renderers.Count > 0) // There will be one renderer component with all meshes in it
			{
				rootNode.AddComponent(Renderers[0]);
				rootNode.Name = Renderers[0].TargetMeshRenderers[0].gameObject.name;

				if (Renderers[0].skinned)
				{
					var rigRoot = (Renderers[0].TargetMeshRenderers[0] as SkinnedMeshRenderer).rootBone;
					var rigRootNode = SceneNode.BuildSceneTree(rigRoot, out var boneNodes);
					var rig = new RigSceneComponent
					{
						Node = rootNode,
						Bones = boneNodes,
						Root = rigRootNode
					};

					rootNode.AddComponent(rig);
				}
			}
			
			parent.AddChild(rootNode);
			
			ApplyRuntimeConfig(runtimeConfig);
			
			WriteOutput("Renderer", Renderers.FirstOrDefault());
			WriteOutput("Scene Node", rootNode);
		}

		private IEnumerator LoadLodResource(ResourceId resourceId)
		{
			var routine = CoroutineHost.Instance.StartCoroutine(
				NodeContext.ExecutionContext.Config.GetMeshInstance(
					resourceId,
					MeshLoadedCallback
				)
			);
			
			if (routine != null)
			{
				yield return routine;
			}
		}
		
		private void MeshLoadedCallback(GltfImport resource, MeshAssetImportSettings settings)
		{
			var existingLod = _lodData.FirstOrDefault(data => data.Mesh.IsEqual(resource));
			if (existingLod != null)
			{
				existingLod.MeshNames.Add(settings.LODMeshIdentifier);
			}
			else
			{
				_lodData.Add(new LodData()
				{
					Mesh = resource,
					MeshNames = new List<string>{settings.LODMeshIdentifier},
				});
			}
		}

		private IEnumerator SpawnLodMesh(LodData lodData, Transform parent)
		{
			var glbReference = parent.gameObject.AddComponent<GLBReference>();
			glbReference.GLTFImport = lodData.Mesh;
			
			var instantiator = new UbfMeshInstantiator(lodData.Mesh, parent, lodData.MeshNames);
			instantiator.MeshAdded += MeshAddedCallback;

			return new WaitForTask(lodData.Mesh.InstantiateMainSceneAsync(instantiator));
		}
		
		protected override void MeshAddedCallback(
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

			if (Renderers.Count == 0) // Only one mesh renderer on an lod spawn, that contains all the lod meshes within it
			{
				Renderers.Add(new MeshRendererSceneComponent()
				{
					skinned = renderer is SkinnedMeshRenderer
				});
			}

			Renderers[0].TargetMeshRenderers.Add(renderer);
			
			var lodSample = (_currentLodNum + 1) / (float)_numLods;
			var lodDistanceFactor = UBFSettings.GetOrCreateSettings()
				.LodFalloffCurve.Evaluate(lodSample);
			var lod = new LOD(
				lodDistanceFactor,
				new[]
				{
					renderer,
				}
			);
			_lods[_currentLodNum++] = lod;
		}
	}
}