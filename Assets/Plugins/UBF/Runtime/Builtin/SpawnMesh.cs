// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
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

			WriteOutput("Renderers", renderersArray);
			WriteOutput("Scene Nodes", sceneNodesArray);
			TriggerNext();
		}
	}
}