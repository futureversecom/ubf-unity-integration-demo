// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using Futureverse.UBF.ExecutionController.Runtime;
using Futureverse.UBF.Runtime.Execution;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Futureverse.UBF.ExecutionController.Samples.LoadFromAssetProfile
{
#if UNITY_EDITOR
	[CustomEditor(typeof(AssetProfileExecutor))]

	public class AssetProfileExecutorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			GUILayout.Space(8);
			if (GUILayout.Button("Run"))
			{
				var executor = (AssetProfileExecutor)target;
				executor.Run();
			}
		}
	}
#endif

	public class AssetProfileExecutor : MonoBehaviour
	{
		[SerializeField] private FutureverseRuntimeController _controller;
		[SerializeField] private string _chainId;
		[SerializeField] private string _chainName;
		[SerializeField] private string _collectionId;
		[SerializeField] private string _tokenId;
		[SerializeField] private string _metadata;
		[SerializeField] private bool _overrideSupportedVariants;
		[SerializeField] private string[] _supportedVariantOverrides;

		[ContextMenu("Run")]
		public void Run()
		{
			var assetData = new AssetData(
				$"{_collectionId}:{_tokenId}",
				_chainId,
				_chainName,
				_collectionId,
				_tokenId,
				_metadata
			);
			var assetTree = new AssetTree(assetData);

			var dataParser = new AssetProfileDataParser(_overrideSupportedVariants ? _supportedVariantOverrides : null);
			StartCoroutine(_controller.ExecuteAssetTree(assetTree, dataParser, OnExecutionFinished));
		}
		
		private void OnExecutionFinished(ExecutionResult result)
		{
			Debug.Log($"Blueprint execution completed with a result of {(result.Success ? "Success" : "Failure")}!");
		}
	}
}