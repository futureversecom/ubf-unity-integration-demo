// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Resources;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Testbed.Local
{
#if UNITY_EDITOR
	[CustomEditor(typeof(CompositeExecutor))]

	public class CompositeExecutorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			GUILayout.Space(18);
			if (GUILayout.Button("Run"))
			{
				var executor = (CompositeExecutor)target;
				executor.Run();
			}

			if (GUILayout.Button("Delete"))
			{
				var executor = (CompositeExecutor)target;
				executor.Delete();
			}
		}
	}
#endif
	
	public class CompositeExecutor : MonoBehaviour
	{
		[SerializeField] private AssetDefinition[] _assets;
		
		public void Run()
		{
			StartCoroutine(RunRoutine());
		}

		public void Delete()
		{
			foreach (Transform child in transform)
			{
				Destroy(child.gameObject);
			}

			Resources.UnloadUnusedAssets();
		}

		private IEnumerator RunRoutine()
		{
			Delete();

			if (_assets.Length == 0)
			{
				yield break;
			}

			var artifactProvider = new ArtifactProvider();
			var blueprints = new List<IBlueprintInstanceData>();

			foreach (var asset in _assets)
			{
				Catalog catalog = null;
				var catalogLoader = new JsonResourceLoader<Catalog>(asset.CatalogUri);
				yield return catalogLoader.Get(c => catalog = c);
				if (catalog == null)
				{
					Debug.LogError($"[UBF] Failed to load catalog, aborting");
					yield break;
				}
				artifactProvider.RegisterCatalog(catalog);

				var blueprintDefinition = new TestBlueprintInstanceData(asset.InstanceId, asset.BlueprintId);
				foreach (var blueprintInput in asset.Inputs)
				{
					if (blueprintInput != null)
					{
						blueprintDefinition.AddInput(blueprintInput.Key, blueprintInput.GetValue);
					}
				}
				blueprints.Add(blueprintDefinition);
			}
			
			var executionData = new ExecutionData(
				transform,
				null,
				blueprints,
				artifactProvider
			);
			yield return UBFExecutor.ExecuteRoutine(executionData, _assets[0].InstanceId);
		}
	}
}