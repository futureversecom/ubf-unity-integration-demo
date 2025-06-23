// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Resources;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Testbed.Simple
{
#if UNITY_EDITOR
	[CustomEditor(typeof(SimpleExecutor))]

	public class SimpleExecutorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.Space(18);
			if (GUILayout.Button("Run"))
			{
				var executor = (SimpleExecutor)target;
				executor.Run();
			}

			if (GUILayout.Button("Delete"))
			{
				var executor = (SimpleExecutor)target;
				executor.Delete();
			}
		}
	}
#endif

	public class SimpleExecutor : MonoBehaviour
	{
		[SerializeField] private string _catalogUri;
		[SerializeField] private string _blueprintId;

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

			Catalog catalog = null;
			var catalogLoader = new JsonResourceLoader<Catalog>(_catalogUri);
			yield return catalogLoader.Get(c => catalog = c);
			if (catalog == null)
			{
				Debug.LogError("[UBF] Failed to load catalog, aborting");
				yield break;
			}

			var artifactProvider = new ArtifactProvider();
			artifactProvider.RegisterCatalog(catalog);

			var blueprintDefinition = new BlueprintInstanceData(_blueprintId);
			var blueprints = new List<IBlueprintInstanceData>
			{
				blueprintDefinition
			};

			var executionData = new ExecutionData(
				transform,
				null,
				blueprints,
				artifactProvider
			);
			yield return UBFExecutor.ExecuteRoutine(executionData, blueprintDefinition.InstanceId);
		}
	}
}