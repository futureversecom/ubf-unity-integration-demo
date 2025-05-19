// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using UnityEngine;

namespace Futureverse.UBF.ExecutionController.Runtime.Settings
{
	internal class ExecutionControllerSettings : ScriptableObject
	{
		private const string MyCustomSettingsPath = "Assets/Settings/ExecutionControllerSettings.asset";

		[SerializeField] private string _assetProfilesPath;
		[SerializeField] [Tooltip("List of Variants that can be loaded, in order of load preference")]
		private string[] _supportedVariants;

		public string AssetProfilesPath => _assetProfilesPath;
		public string[] SupportedVariants => _supportedVariants;

		public static ExecutionControllerSettings GetOrCreateSettings()
		{
#if UNITY_EDITOR
			var settings = AssetDatabase.LoadAssetAtPath<ExecutionControllerSettings>(MyCustomSettingsPath);
			if (settings != null)
			{
				return settings;
			}

			var fullPath = $"{Application.dataPath}/Settings";
			if (!Directory.Exists(fullPath))
			{
				Directory.CreateDirectory(fullPath);
			}

			settings = CreateInstance<ExecutionControllerSettings>();
			AssetDatabase.CreateAsset(settings, MyCustomSettingsPath);
			AssetDatabase.SaveAssets();
			return settings;
#endif
#pragma warning disable CS0162
			return null;
#pragma warning restore CS0162
		}

#if UNITY_EDITOR
		internal static SerializedObject GetSerializedSettings()
			=> new(GetOrCreateSettings());
#endif
	}
}