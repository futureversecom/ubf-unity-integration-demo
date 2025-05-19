// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Resources;
using UnityEngine;

namespace Futureverse.UBF.ExecutionController.Runtime
{
	/// <summary>
	/// None - No caching.
	/// InMemory - Uses caching that exists as long as the containing object does.
	/// ReadThrough - Stores cached data on the users disk.
	/// </summary>
	public enum CacheType
	{
		None,
		InMemory,
		ReadThrough,
	}

	/// <summary>
	/// Extension of UBFRuntimeController that can turns an Asset tree into ExecutionData that can be run.
	/// </summary>
	public class FutureverseRuntimeController : UBFRuntimeController
	{
		[SerializeField] private CacheType _cacheType = CacheType.None;
		[SerializeField] private string _readThroughCachePathOverride;

		private ICache _cache;

		private void Awake()
		{
			_cache = _cacheType switch
			{
				CacheType.ReadThrough => new ReadThroughCache(
					string.IsNullOrEmpty(_readThroughCachePathOverride) ?
						Application.temporaryCachePath :
						_readThroughCachePathOverride
				),
				CacheType.InMemory => new InMemoryCache(),
				_ => null,
			};
		}

		/// <summary>
		/// Turns an asset tree structure into the data required to Execute each Blueprint.
		/// </summary>
		/// <param name="assetTree">Tree structure containing the asset data and child links.</param>
		/// <param name="parser">Describes how the UBF data is interpreted.</param>
		/// <param name="onComplete">Callback containing the result of the Blueprint execution.</param>
		/// <returns>An IEnumerator to yield on.</returns>
		public IEnumerator ExecuteAssetTree(
			IUbfTree assetTree,
			IUbfDataParser parser,
			Action<ExecutionResult> onComplete)
		{
			var artifactProvider = new ArtifactProvider(_cache);
			var blueprintDefinitions = new Dictionary<string, IBlueprintInstanceData>();
			var rootInstanceId = "";

			foreach (var node in assetTree.TreeNodes)
			{
				yield return parser.GetBlueprintDefinition(
					node.NodeData,
					(definition, catalog) =>
					{
						if (definition == null || catalog == null)
						{
							return;
						}
						
						blueprintDefinitions.Add(node.NodeData.Id, definition);
						artifactProvider.RegisterCatalog(catalog);

						if (node.NodeData == assetTree.RootData)
						{
							rootInstanceId = definition.InstanceId;
						}
					}
				);
			}

			foreach (var node in assetTree.TreeNodes)
			{
				if (!blueprintDefinitions.TryGetValue(node.NodeData.Id, out var blueprintDefinition) ||
					blueprintDefinition is not BlueprintInstanceData blueprint)
				{
					Debug.LogWarning($"Could not find blueprint definition for {node.NodeData.Id}");
					continue;
				}

				foreach (var child in node.Children)
				{
					var childElement = assetTree.TreeNodes.FirstOrDefault(e => e.NodeData == child.Value);
					if (childElement == null)
					{
						Debug.LogError("Can't find child graph!");
						continue;
					}

					if (!blueprintDefinitions.TryGetValue(childElement.NodeData.Id, out var childDefinition) ||
						childDefinition is not BlueprintInstanceData childInstance)
					{
						Debug.LogWarning($"Could not find blueprint definition for {node.NodeData.Id}");
						continue;
					}

					blueprint.AddInput(child.Key, childInstance.InstanceId);
				}
			}

			yield return Execute(
				rootInstanceId,
				artifactProvider,
				blueprintDefinitions.Values.ToList(),
				onComplete
			);
		}
	}
}