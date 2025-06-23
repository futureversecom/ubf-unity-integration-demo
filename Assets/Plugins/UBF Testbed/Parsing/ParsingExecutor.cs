// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Resources;
using UnityEngine;

namespace Testbed.Parsing
{
#if UNITY_EDITOR
	using UnityEditor;
	[CustomEditor(typeof(ParsingExecutor))]

	public class ParsingExecutorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			GUILayout.Space(18);
			if (GUILayout.Button("Run"))
			{
				var executor = (ParsingExecutor)target;
				executor.Run();
			}

			if (GUILayout.Button("Delete"))
			{
				var executor = (ParsingExecutor)target;
				executor.Delete();
			}
		}
	}
#endif
	
	public class ParsingExecutor : MonoBehaviour
	{
		[SerializeField] private string _parsingCatalogUri;
		[SerializeField] private string _parsingBlueprintId;
		[SerializeField] private string _renderCatalogUri;
		[SerializeField] private string _renderBlueprintId;
		[SerializeField] private string _metadata;
		
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
			
			Catalog parsingCatalog = null;
			var parsingCatalogLoader = new JsonResourceLoader<Catalog>(_parsingCatalogUri);
			yield return parsingCatalogLoader.Get(c => parsingCatalog = c);
			if (parsingCatalog == null)
			{
				Debug.LogError("[UBF] Failed to load parsing catalog, aborting");
				yield break;
			}

			var parsingArtifactProvider = new ArtifactProvider();
			parsingArtifactProvider.RegisterCatalog(parsingCatalog);

			var parsingBlueprintDefinition = new BlueprintInstanceData(_parsingBlueprintId);
			parsingBlueprintDefinition.AddInput("metadata", _metadata);
			var parsingBlueprints = new List<IBlueprintInstanceData>
			{
				parsingBlueprintDefinition,
			};

			ExecutionResult parsingResult = null;
			var parsingExecutionData = new ExecutionData(
				transform,
				r => parsingResult = r,
				parsingBlueprints,
				parsingArtifactProvider
			);
			yield return UBFExecutor.ExecuteRoutine(parsingExecutionData, parsingBlueprintDefinition.InstanceId);
			
			
			
			
			Catalog renderCatalog = null;
			var renderCatalogLoader = new JsonResourceLoader<Catalog>(_renderCatalogUri);
			yield return renderCatalogLoader.Get(c => renderCatalog = c);
			if (renderCatalog == null)
			{
				Debug.LogError("[UBF] Failed to load render catalog, aborting");
				yield break;
			}

			var renderArtifactProvider = new ArtifactProvider();
			renderArtifactProvider.RegisterCatalog(renderCatalog);

			var renderBlueprintDefinition = new BlueprintInstanceData(_renderBlueprintId);
			foreach (var output in parsingResult.BlueprintOutputs)
			{
				renderBlueprintDefinition.AddInput(output.Key, output.Value);
			}
			
			var renderBlueprints = new List<IBlueprintInstanceData>
			{
				renderBlueprintDefinition,
			};

			var renderExecutionData = new ExecutionData(
				transform,
				null,
				renderBlueprints,
				renderArtifactProvider
			);
			yield return UBFExecutor.ExecuteRoutine(renderExecutionData, renderBlueprintDefinition.InstanceId);
		}
	}
}