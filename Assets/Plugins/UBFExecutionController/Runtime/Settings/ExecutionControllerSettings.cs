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
		private const string MyCustomSettingsPath = "Assets/Resources/RuntimeSettings/ExecutionControllerSettings.asset";

		[SerializeField] private bool _useAssetRegisterProfiles;
		[SerializeField] private string _assetProfilesPath;
		[SerializeField] [Tooltip("List of Variants that can be loaded, in order of load preference")]
		private string[] _supportedVariants;

		public string AssetProfilesPath => _assetProfilesPath;
		public string[] SupportedVariants => _supportedVariants;
		public bool UseAssetRegisterProfiles => _useAssetRegisterProfiles;

		public static ExecutionControllerSettings GetOrCreateSettings()
		{
			var settings = Resources.Load<ExecutionControllerSettings>("RuntimeSettings/ExecutionControllerSettings");
			
			if (settings == null)
			{
#if UNITY_EDITOR
				const string fullPath = "Assets/Resources/RuntimeSettings";
				if (!Directory.Exists(fullPath))
				{
					Directory.CreateDirectory(fullPath);
				}

				settings = CreateInstance<ExecutionControllerSettings>();
				AssetDatabase.CreateAsset(settings, MyCustomSettingsPath);
				AssetDatabase.SaveAssets();
#endif
			}
		
			return settings;
		}

#if UNITY_EDITOR
		internal static SerializedObject GetSerializedSettings()
			=> new(GetOrCreateSettings());
#endif
	}
}