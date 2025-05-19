// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Futureverse.UBF.ExecutionController.Runtime.Settings
{
	internal static class ExecutionControllerSettingsProvider
	{
#if UNITY_EDITOR
		[SettingsProvider]
		public static SettingsProvider CreateMyCustomSettingsProvider()
		{
			var provider = new SettingsProvider("Project/UBFExecutionController", SettingsScope.Project)
			{
				label = "UBF Execution Controller",
				guiHandler = _ =>
				{
					EditorGUI.BeginChangeCheck();
					var settings = ExecutionControllerSettings.GetSerializedSettings();
					EditorGUILayout.PropertyField(settings.FindProperty("_assetProfilesPath"));
					EditorGUILayout.PropertyField(settings.FindProperty("_supportedVariants"));
					if (EditorGUI.EndChangeCheck())
					{
						settings.ApplyModifiedProperties();
					}
				},

				keywords = new HashSet<string>(
					new[]
					{
						"AssetProfile",
						"Asset",
						"Profile",
						"Path",
						"UBF",
						"Execution",
						"Futureverse",
					}
				),
			};

			return provider;
		}
#endif
	}
}