// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using GLTFast;
using Newtonsoft.Json;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Resources
{
	public interface IAssetImportSettings<T>
	{
		
	}
	
	public class EmptyImportSettings<T> : IAssetImportSettings<T>
	{
		
	}

	[JsonObject]
	public class MeshAssetImportSettings : IAssetImportSettings<GltfImport>
	{
		public string LODMeshIdentifier => _meshIdentifier;
		
		[JsonProperty(PropertyName = "MeshIdentifier")]
		private string _meshIdentifier;
	}
	
	[JsonObject]
	public class TextureAssetImportSettings : IAssetImportSettings<Texture2D>
	{
		public bool IsSrgb => _isSrgb;
		
		[JsonProperty(PropertyName = "isSrgb")]
		private bool _isSrgb;
	}
	
	[JsonObject]
	public class BlueprintAssetImportSettings : IAssetImportSettings<Blueprint>
	{
		
	}
}