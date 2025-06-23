// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.ExecutionController.Runtime;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Resources;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Testbed.Simple
{
#if UNITY_EDITOR
	[CustomEditor(typeof(AssetExecutor))]

	public class AssetExecutorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.Space(18);
			if (GUILayout.Button("Run"))
			{
				var executor = (AssetExecutor)target;
				executor.Run();
			}

			if (GUILayout.Button("Delete"))
			{
				var executor = (AssetExecutor)target;
				executor.Delete();
			}
		}
	}
#endif

	public class AssetExecutor : MonoBehaviour
	{
		[SerializeField] private string _chainId;
		[SerializeField] private string _chainName;
		[SerializeField] private string _collectionId;
		[SerializeField] private string _tokenId;

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

			AssetProfile profile = null;
			yield return AssetProfile.FetchByAssetId(
				_chainId,
				_chainName,
				_collectionId,
				_tokenId,
				assetProfile => profile = assetProfile
			);
			if (profile == null)
			{
				Debug.LogError("Failed to fetch asset profile");
				yield break;
			}
			
			var artifactProvider = new ArtifactProvider();
			artifactProvider.RegisterCatalog(profile.RenderCatalog);

			var blueprintDefinition = new BlueprintInstanceData(profile.RenderBlueprintResourceId);
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