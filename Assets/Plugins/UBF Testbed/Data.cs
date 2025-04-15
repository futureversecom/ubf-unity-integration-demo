// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Testbed
{
	// [JsonObject]
	// public class AssetTreeData : IUbfTree
	// {
	// 	[JsonObject]
	// 	public class Entry : IUbfData
	// 	{
	// 		public string TokenId => _assetId.Split(":")[1];
	// 		public string CollectionId => _assetId.Split(":")[0];
	// 		public JObject Metadata => _metadata;
	// 		public string Id => _assetId;
	// 		public Dictionary<string, string> Children => _children;
	// 		
	// 		[JsonProperty(PropertyName = "metadata")]
	// 		private JObject _metadata;
	// 		[JsonProperty(PropertyName = "asset-id")]
	// 		private string _assetId;
	// 		[JsonProperty(PropertyName = "children")]
	// 		private Dictionary<string, string> _children;
	// 	}
	//
	// 	public class TreeNode : IUbfTree.IUbfTreeNode
	// 	{
	// 		public IUbfData NodeData { get; }
	// 		public Dictionary<string, IUbfData> Children { get; }
	// 	}
	//
	// 	public string RootAssetId => _rootId;
	// 	public IUbfTree.IUbfTreeNode[] TreeNodes => _entries;
	// 	
	// 	[JsonProperty(PropertyName = "entries")]
	// 	private Entry[] _entries;
	// 	[JsonProperty(PropertyName = "root-id")]
	// 	private string _rootId;
	// }
}