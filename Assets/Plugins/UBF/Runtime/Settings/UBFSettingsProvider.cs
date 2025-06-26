// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Futureverse.UBF.Runtime.Settings
{
	internal class UBFSettingsProvider
	{
#if UNITY_EDITOR
		[SettingsProvider]
		public static SettingsProvider CreateMyCustomSettingsProvider()
		{
			var provider = new SettingsProvider("Project/UBF", SettingsScope.Project)
			{
				label = "UBF",
				guiHandler = _ =>
				{
					EditorGUILayout.LabelField("Override these materials if you want to use custom material in UBF");
					EditorGUI.BeginChangeCheck();
					var settings = UBFSettings.GetSerializedSettings();
					EditorGUILayout.PropertyField(settings.FindProperty("_decalOpaque"));
					EditorGUILayout.PropertyField(settings.FindProperty("_decalTransparent"));
					EditorGUILayout.PropertyField(settings.FindProperty("_furOpaque"));
					EditorGUILayout.PropertyField(settings.FindProperty("_furTransparent"));
					EditorGUILayout.PropertyField(settings.FindProperty("_pbrOpaque"));
					EditorGUILayout.PropertyField(settings.FindProperty("_pbrTransparent"));
					EditorGUILayout.PropertyField(settings.FindProperty("_hair"));
					EditorGUILayout.PropertyField(settings.FindProperty("_skin"));
					EditorGUILayout.PropertyField(settings.FindProperty("_skin02"));
					
					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Add mesh config data here to support custom rigs at runtime");
					EditorGUILayout.PropertyField(settings.FindProperty("_meshConfigs"));
					
					EditorGUILayout.LabelField("LOD Settings");	
					EditorGUILayout.PropertyField(settings.FindProperty("_lodFalloffCurve"));
					if (EditorGUI.EndChangeCheck())
					{
						settings.ApplyModifiedProperties();
					}
				},

				keywords = new HashSet<string>(
					new[]
					{
						"UBF",
						"Futureverse",
						"MeshConfig",
						"Material"
					}
				),
			};

			return provider;
		}
#endif
	}
}