// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Futureverse.UBF.ExecutionController.Runtime;
using Newtonsoft.Json.Linq;

namespace Futureverse.UBF.ExecutionController.Samples.LoadFromAssetProfile
{
	public class AssetData : IUbfAsset
	{
		public string Id { get; }
		public string CollectionId { get; }
		public JObject Metadata { get; }
		public string TokenId { get; }

		public AssetData(
			string id,
			string collectionId,
			string tokenId,
			string metadata)
		{
			Id = id;
			CollectionId = collectionId;
			TokenId = tokenId;
			Metadata = string.IsNullOrEmpty(metadata) ? null : JObject.Parse(metadata);
		}
	}
	
	public class AssetTreeNode : IUbfTree.IUbfTreeNode
	{
		public IUbfData NodeData { get; }
		public Dictionary<string, IUbfData> Children { get; }

		public AssetTreeNode(AssetData nodeData)
		{
			NodeData = nodeData;
			Children = new Dictionary<string, IUbfData>();
		}
	}

	public class AssetTree : IUbfTree
	{
		public IUbfData RootData { get; }
		public IUbfTree.IUbfTreeNode[] TreeNodes { get; }

		public AssetTree(AssetData rootData)
		{
			RootData = rootData;
			TreeNodes = new IUbfTree.IUbfTreeNode[1];
			TreeNodes[0] = new AssetTreeNode(rootData);
		}
	}
}