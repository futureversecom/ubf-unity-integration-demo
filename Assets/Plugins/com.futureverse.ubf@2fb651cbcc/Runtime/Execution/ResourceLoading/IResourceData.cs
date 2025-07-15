// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Futureverse.UBF.Runtime.Resources
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ResourceType
	{
		[EnumMember(Value = "")]
		Unspecified,
		[EnumMember(Value = "blueprint")]
		Blueprint,
		[EnumMember(Value = "glb")]
		GLB,
		[EnumMember(Value = "mesh")]
		Mesh,
		[EnumMember(Value = "texture")]
		Texture,
	}
	
	/// <summary>
	/// Represents a cacheable remote resource that can be accessed via a URI.
	/// </summary>
	public interface IResourceData
	{
		/// <summary>
		/// Used for caching. If a cached resource exists with the given hash, the resource is not downloaded
		/// again, and the cached version is used. It is recommended to change the resource hash whenever the
		/// resource is updated, so that new versions are always downloaded and re-cached.
		/// </summary>
		string Hash { get; }
		/// <summary>
		/// Either a URL or local path that points to the target resource.
		/// </summary>
		string Uri { get; }
		ResourceType Type { get; }
		JObject ImportSettings { get; }
		string StandardVersion { get; }
	}
}