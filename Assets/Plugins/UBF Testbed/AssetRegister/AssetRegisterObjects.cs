// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.ExecutionController.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Testbed.AssetRegister
{
	public class InventoryResultData
    {
        [JsonProperty("data")] public InventoryData InventoryData;
    }

    public class InventoryData
    {
        [JsonProperty("assets")] public InventoryAssets InventoryAssets;
    }

    public class InventoryAssets
    {
        [JsonProperty("edges")] public InventoryEdge[] Edges;
    }

    public class InventoryEdge
    {
        [JsonProperty("node")] public InventoryNode InventoryNode;
    }

    [JsonObject]
    public class InventoryNode : IUbfAsset
    {
        [JsonProperty("collectionId")] private string _collectionId;
        [JsonProperty("tokenId")] private string _tokenId;
        [JsonProperty("assetType")] private string _assetType;
        [JsonProperty("metadata")] private JObject _metadata;
        [JsonProperty("collection")] private ChainData _collection;
        [JsonProperty("assetTree")] public AssetTree AssetTree { get; set; }

        public Dictionary<string, string> EquippedAssets
            => _equippedAssets ??= AssetTree.Data.Graph.SelectMany(
                    dict => dict,
                    (_, kvp) => new
                    {
                        kvp.Key,
                        kvp.Value,
                    }
                )
                .Where(kvp => kvp.Key.StartsWith("path:"))
                .ToDictionary(
                    kvp => kvp.Key[5..].Replace("_accessory", ""),
                    kvp => kvp.Value.ToObject<JObject>()
                        .GetValue("@id")?
                        .ToString()
                        .Replace($"did:fv-asset:{_collection.ChainId}:{_collection.ChainType}:", "")
                );

        public string Id => $"{CollectionId}:{_tokenId}";
        public string AssetName
            => _metadata["properties"]?["name"]
                ?.ToString();
        public string SanitizedAssetName
            => AssetName.ToLower()
                .Replace(" ", "")
                .Replace("-", "");
        public string TokenId => _tokenId;
        public string CollectionId => _collectionId.Split(":")[^1];
        
        // For some reason, Unreal project passes in a higher level object where metadata is expected to be a property.
        // So we're wrapping metadata in a new JObject so the parsing BP has access to a 'metadata' property.
        public JObject Metadata
        {
            get
            {
                if (_higherLevelMetadata == null)
                {
                    _higherLevelMetadata = new JObject();
                    _higherLevelMetadata.Add("metadata", _metadata);
                }

                return _higherLevelMetadata;
            }
        }

        private Dictionary<string, string> _equippedAssets;
        private JObject _higherLevelMetadata;
    }
    
    public class InventoryQueryVariables
    {
        [JsonProperty("addresses")] public string[] Addresses;
        [JsonProperty("collectionIds")] public string[] CollectionIds;
        [JsonProperty("first")] public int First;
    }

    public class ChainData
    {
        [JsonProperty("chainId")] public int ChainId;
        [JsonProperty("chainType")] public string ChainType;
    }

    public class AssetTree
    {
        [JsonProperty("data")] public AssetTreeData Data { get; set; }
    }

    public class AssetTreeData
    {
        [JsonProperty("@graph")] public List<Dictionary<string, JToken>> Graph { get; set; }
    }

    public class ARAssetTree : IUbfTree
    {
        public IUbfData RootData { get; }
        public IUbfTree.IUbfTreeNode[] TreeNodes => _treeNodes.ToArray();
        
        private readonly List<IUbfTree.IUbfTreeNode> _treeNodes = new();
        
        public ARAssetTree(InventoryNode rootInventoryNode)
        {
            RootData = rootInventoryNode;
        }

        public void RegisterElement(InventoryNode child, Dictionary<string, IUbfData> children)
        {
            var treeNode = new ARAssetTreeNode(child, children);
            _treeNodes.Add(treeNode);
        }
    }

    public class ARAssetTreeNode : IUbfTree.IUbfTreeNode
    {
        public IUbfData NodeData { get; }
        public Dictionary<string, IUbfData> Children { get; }

        public ARAssetTreeNode(InventoryNode nodeData, Dictionary<string, IUbfData> children)
        {
            NodeData = nodeData;
            Children = children;
        }
    }
}