// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Settings
{
	public class UBFSettings : ScriptableObject
	{
		private const string MyCustomSettingsPath = "Assets/Resources/UBF.asset";

		[SerializeField] private Material _decalOpaque;
		[SerializeField] private Material _decalTransparent;
		[SerializeField] private Material _furOpaque;
		[SerializeField] private Material _furTransparent;
		[SerializeField] private Material _pbrOpaque;
		[SerializeField] private Material _pbrTransparent;
		[SerializeField] private Material _hair;
		[SerializeField] private Material _skin;
		[SerializeField] private Material _skin02;
		[SerializeField] private List<MeshConfigEntry> _meshConfigs;
		[SerializeField] private AnimationCurve _lodFalloffCurve;
		public Material DecalOpaque => _decalOpaque;
		public Material DecalTransparent => _decalTransparent;
		public Material FurOpaque => _furOpaque;
		public Material FurTransparent => _furTransparent;
		public Material PbrOpaque => _pbrOpaque;
		public Material PbrTransparent => _pbrTransparent;
		public Material Hair => _hair;
		public Material Skin => _skin;
		public Material Skin02 => _skin02;
		public AnimationCurve LodFalloffCurve => _lodFalloffCurve;

		[Serializable]
		public class MeshConfigEntry
		{
			public string Key;
			public MeshConfig Config;
		}

		public List<MeshConfigEntry> MeshConfigs => _meshConfigs;
		
		public static UBFSettings GetOrCreateSettings()
		{
#if UNITY_EDITOR
			var settings = AssetDatabase.LoadAssetAtPath<UBFSettings>(MyCustomSettingsPath);
			if (settings != null)
			{
				return settings;
			}

			var fullPath = $"{Application.dataPath}/Resources";
			if (!Directory.Exists(fullPath))
			{
				Directory.CreateDirectory(fullPath);
			}

			settings = CreateInstance<UBFSettings>();
			AssetDatabase.CreateAsset(settings, MyCustomSettingsPath);
			AssetDatabase.SaveAssets();
			return settings;
#else
			return UnityEngine.Resources.Load<UBFSettings>("UBF");
#endif
		}

		private void OnEnable()
		{
			OnValidate();
		}

		private void OnValidate()
		{
			if (_decalOpaque == null)
			{
				_decalOpaque = UnityEngine.Resources.Load("Materials/M_Decal_Opaque") as Material;
			}
			if (_decalTransparent == null)
			{
				_decalTransparent = UnityEngine.Resources.Load("Materials/M_Decal_Transparent_Alpha") as Material;
			}
			if (_furOpaque == null)
			{
				_furOpaque = UnityEngine.Resources.Load("Materials/M_Fur_Opaque") as Material;
			}
			if (_furTransparent == null)
			{
				_furTransparent = UnityEngine.Resources.Load("Materials/M_Fur_Transparent_Alpha") as Material;
			}
			if (_pbrOpaque == null)
			{
				_pbrOpaque = UnityEngine.Resources.Load("Materials/M_PBR_Opaque") as Material;
			}
			if (_pbrTransparent == null)
			{
				_pbrTransparent = UnityEngine.Resources.Load("Materials/M_PBR_Transparent_Alpha") as Material;
			}
			if (_hair == null)
			{
				_hair = UnityEngine.Resources.Load("Materials/M_Hair_Transparent_Alpha_Clipping") as Material;
			}
			if (_skin == null)
			{
				_skin = UnityEngine.Resources.Load("Materials/M_Skin_Opaque") as Material;
			}
			if (MeshConfigs == null)
			{
				_meshConfigs = new List<MeshConfigEntry>();
			}

			if (_lodFalloffCurve == null || _lodFalloffCurve.length < 2)
			{
				_lodFalloffCurve = new AnimationCurve();
				_lodFalloffCurve.AddKey(0, 1);
				_lodFalloffCurve.AddKey(1, 0);
			}
		}
		
#if UNITY_EDITOR
		internal static SerializedObject GetSerializedSettings()
			=> new(GetOrCreateSettings());
#endif
	}
}