// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using GLTFast;
using Newtonsoft.Json;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Resources
{
	public abstract class AAssetImportSettings<T>
	{
		public Version StandardVersion { get; set; }
	}
	
	public class EmptyImportSettings<T> : AAssetImportSettings<T>
	{
		
	}

	[JsonObject]
	public class MeshAssetImportSettings : AAssetImportSettings<GltfImport>
	{
		public string LODMeshIdentifier => _meshIdentifier;
		
		[JsonProperty(PropertyName = "MeshIdentifier")]
		private string _meshIdentifier;
	}
	
	[JsonObject]
	public class TextureAssetImportSettings : AAssetImportSettings<Texture2D>
	{
		public bool IsSrgb => _isSrgb;
		
		[JsonProperty(PropertyName = "isSrgb")]
		private bool _isSrgb;
	}
	
	[JsonObject]
	public class BlueprintAssetImportSettings : AAssetImportSettings<Blueprint>
	{
		
	}
}