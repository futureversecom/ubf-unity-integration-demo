// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.IO;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Resources;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Futureverse.UBF.Samples.SumOfTwoNumbers
{
#if UNITY_EDITOR
	[CustomEditor(typeof(SumExecutor))]

	public class ExampleExecutorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.Space(8);
			if (GUILayout.Button("Run"))
			{
				var executor = (SumExecutor)target;
				executor.Run();
			}
		}
	}
#endif

	public class SumExecutor : MonoBehaviour
	{
		[SerializeField] private int _a;
		[SerializeField] private int _b;

		[ContextMenu("Run")]
		public void Run()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			
			const string blueprintId = "my-blueprint";
			var blueprintPath = Path.Combine(Application.dataPath, "Plugins/UBF/Samples/SumOfTwoNumbers/SumBlueprint.json");

			// Create a catalog and add a blueprint resource using SumBlueprint's URI.
			var catalog = new Catalog();
			var blueprintResource = new ResourceData(blueprintId, blueprintPath);
			catalog.AddResource(blueprintResource);

			// Register the created catalog to the artifact provider.
			ArtifactProvider.Instance.RegisterCatalog(catalog);

			// Create the blueprint instance data by passing in the blueprintId and adding two input variables.
			var blueprintDefinition = new BlueprintInstanceData(blueprintId);
			blueprintDefinition.AddInput("A", _a);
			blueprintDefinition.AddInput("B", _b);

			// Create the execution data object and run the ExecuteRoutine coroutine.
			var blueprints = new List<IBlueprintInstanceData>
			{
				blueprintDefinition,
			};
			
			var executionData = new ExecutionData(
				transform,
				OnExecutionFinished,
				blueprints
			);
			StartCoroutine(UBFExecutor.ExecuteRoutine(executionData, blueprintDefinition.InstanceId));
		}

		private void OnExecutionFinished(ExecutionResult result)
		{
			Debug.Log($"Blueprint execution completed with a result of {(result.Success ? "Success" : "Failure")}!");
			
			// If the Blueprint outputs contain our sum result, get that result and print it.
			if (result.BlueprintOutputs.TryGetValue("Result", out var mathResult))
			{
				Debug.Log($"Result is: {mathResult}");
			}
		}
	}
}