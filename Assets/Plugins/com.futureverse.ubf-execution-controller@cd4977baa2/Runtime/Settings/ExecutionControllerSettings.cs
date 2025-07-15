// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Resources;
using Futureverse.UBF.UBFExecutionController.Runtime;
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
		[SerializeReference] private List<IDownloader> _downloaders;
		[SerializeField] private CacheType _cacheType;
		[SerializeField, Tooltip("If blank, uses Application.temporaryCachePath by default")] 
		private string _cachePathOverride;
		[SerializeField] private string _syloResolverUri;
			
		public string AssetProfilesPath => _assetProfilesPath;
		public string[] SupportedVariants => _supportedVariants;
		public bool UseAssetRegisterProfiles => _useAssetRegisterProfiles;
		public List<IDownloader> Downloaders => _downloaders;
		public CacheType CacheType => _cacheType;
		public string CachePathOverride => _cachePathOverride;
		public string SyloResolverUri => _syloResolverUri;

		public static ExecutionControllerSettings GetOrCreateSettings()
		{
			var settings = Resources.Load<ExecutionControllerSettings>("RuntimeSettings/ExecutionControllerSettings");
			
#if UNITY_EDITOR
			if (settings == null)
			{
				const string fullPath = "Assets/Resources/RuntimeSettings";
				if (!Directory.Exists(fullPath))
				{
					Directory.CreateDirectory(fullPath);
				}

				settings = CreateInstance<ExecutionControllerSettings>();
				settings._downloaders = new List<IDownloader>
				{
					new SyloDownloader(),
					new DefaultDownloader(),
				};
				AssetDatabase.CreateAsset(settings, MyCustomSettingsPath);
				AssetDatabase.SaveAssets();
			}
#endif
		
			return settings;
		}

#if UNITY_EDITOR
		internal static SerializedObject GetSerializedSettings()
		{
			var settings = GetOrCreateSettings();
			return new SerializedObject(settings);
		}
#endif
	}
}