// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using Futureverse.UBF.UBFExecutionController.Runtime;
using UnityEditor;

namespace Plugins.UBFExecutionController.Editor
{
	[CustomEditor(typeof(ExecutionController))]
	public class ExecutionControllerEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_arClient"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_ubfController"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_enableForwardLoading"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_syloResolverUri"));

			var specifyCollections = serializedObject.FindProperty("_specifySupportedCollectionIds");
			EditorGUILayout.PropertyField(specifyCollections);
			if (specifyCollections.boolValue)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_supportedCollectionIds"));
			}
			
			var overrideVariants = serializedObject.FindProperty("_overrideSupportedVariants");
			EditorGUILayout.PropertyField(overrideVariants);
			if (overrideVariants.boolValue)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("_variantOverrides"));
			}
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_onFetchAssetsSuccess"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_onFetchAssetsFailure"));

			if (EditorGUI.EndChangeCheck())
			{
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}