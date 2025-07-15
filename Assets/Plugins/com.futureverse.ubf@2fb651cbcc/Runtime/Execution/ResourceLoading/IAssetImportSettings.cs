// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using GLTFast;
using Newtonsoft.Json;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Resources
{
	public abstract class AAssetImportSettings<T>
	{
		[JsonIgnore]
		public Version StandardVersion
		{
			get
			{
				if (_version == null && Version.TryParse(_versionString, out var result))
				{
					_version = result;
				}

				return _version;
			}
			set
			{
				_version = value;
				_versionString = value.ToString();
			}
		}

		private Version _version;

		[JsonProperty("StandardVersion")]
		private string _versionString;
	}
	
	public class EmptyImportSettings<T> : AAssetImportSettings<T>
	{
		
	}

	[JsonObject]
	public class MeshAssetImportSettings : AAssetImportSettings<GltfImport>
	{
		public string LODMeshIdentifier => _meshIdentifier;
		
		[JsonProperty("MeshIdentifier")]
		private string _meshIdentifier;

		[JsonConstructor]
		public MeshAssetImportSettings()
		{
			
		}
		
		public MeshAssetImportSettings(Version standardVersion, string meshIdentifier = null)
		{
			StandardVersion = standardVersion;
			_meshIdentifier = meshIdentifier;
		}
	}
	
	[JsonObject]
	public class TextureAssetImportSettings : AAssetImportSettings<Texture2D>
	{
		public bool IsSrgb => _isSrgb;
		
		[JsonProperty("isSrgb")]
		private bool _isSrgb;
		
		[JsonConstructor]
		public TextureAssetImportSettings()
		{
			
		}
		
		public TextureAssetImportSettings(Version standardVersion, bool isSrgb)
		{
			StandardVersion = standardVersion;
			_isSrgb = isSrgb;
		}
	}

	[JsonObject]
	public class BlueprintAssetImportSettings : AAssetImportSettings<Blueprint>
	{
		[JsonConstructor]
		public BlueprintAssetImportSettings()
		{
			
		}
		
		public BlueprintAssetImportSettings(Version standardVersion)
		{
			StandardVersion = standardVersion;
		}
	}
}