// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using AssetRegister.Runtime.Clients;
using Futureverse.Sylo;
using Futureverse.UBF.ExecutionController.Runtime.Settings;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Resources;
using UnityEngine;

namespace Futureverse.UBF.UBFExecutionController.Runtime
{
	public enum CacheType
	{
		None = 0,
		InMemory = 1,
		OnDisk = 2,
	}
	
	public class ExecutionController : MonoBehaviour
	{
		[SerializeField] private MonoClient _arClient;
		[SerializeField] private UBFRuntimeController _ubfController;

		private void Start()
		{
			var settings = ExecutionControllerSettings.GetOrCreateSettings();
			SyloUtilities.SetResolverUri(settings.SyloResolverUri);
			ICache cache = settings.CacheType switch
			{
				CacheType.OnDisk => new ReadThroughCache(
					string.IsNullOrEmpty(settings.CachePathOverride) ?
						Application.temporaryCachePath :
						settings.CachePathOverride
				),
				CacheType.InMemory => new InMemoryCache(),
				_ => null,
			};
			
			ArtifactProvider.Instance.SetCache(cache);
			foreach (var downloader in settings.Downloaders)
			{
				ArtifactProvider.Instance.RegisterDownloader(downloader);
			}
		}

		/// <summary>
		/// Loads and executes the UBF blueprints for a given inventory item.
		/// </summary>
		/// <param name="item">The inventory item to render</param>
		/// <returns></returns>
		public IEnumerator RenderItem(IInventoryItem item)
		{
			var blueprints = new List<IBlueprintInstanceData>();

			string rootId = null;
			yield return PrepareAssetRecursive(
				item,
				blueprints,
				i => rootId = i.InstanceId
			);

			if (rootId == null)
			{
				Debug.LogError($"Could not load root asset. Aborting");
				yield break;
			}
			
			yield return _ubfController.Execute(rootId, blueprints);
		}
		
		private IEnumerator PrepareAssetRecursive(
			IInventoryItem item,
			List<IBlueprintInstanceData> blueprintInstances,
			Action<IBlueprintInstanceData> callback)
		{
			if (item.AssetProfile == null)
			{
				Debug.LogWarning($"Item {item.Name} does not have an Asset Profile, skipping...");
				yield break;
			}
			
			foreach (var entry in item.AssetProfile.RenderCatalog.Entries)
			{
				ArtifactProvider.Instance.RegisterRuntimeResource(entry.Id, entry);
			}
			
			var renderBlueprintDefinition = new BlueprintInstanceData(item.AssetProfile.RenderBlueprintResourceId);
			
			yield return GetParsingInputs(item.AssetProfile, item.Metadata.ToString(), renderBlueprintDefinition);

			foreach (var (key, value) in item.Children)
			{
				yield return PrepareAssetRecursive(
					value,
					blueprintInstances,
					i => renderBlueprintDefinition.AddInput(key, i.InstanceId)
				);
			}
			
			blueprintInstances.Add(renderBlueprintDefinition);
			callback?.Invoke(renderBlueprintDefinition);
		}

		private IEnumerator GetParsingInputs(AssetProfile profile, string metadata, BlueprintInstanceData renderBlueprint)
		{
			if (profile.ParsingCatalog == null || string.IsNullOrEmpty(profile.ParsingBlueprintResourceId))
			{
				yield break;
			}
			
			var parsingBlueprintDefinition = new BlueprintInstanceData(profile.ParsingBlueprintResourceId);
			parsingBlueprintDefinition.AddInput("metadata", metadata);
			
			foreach (var entry in profile.ParsingCatalog.Entries)
			{
				ArtifactProvider.Instance.RegisterRuntimeResource(entry.Id, entry);
			}

			yield return UBFExecutor.ExecuteRoutine(
				new ExecutionData(
					null,
					result =>
					{
						foreach (var output in result.BlueprintOutputs)
						{
							renderBlueprint.AddInput(output.Key, output.Value);
						}
					},
					new List<IBlueprintInstanceData>
					{
						parsingBlueprintDefinition,
					}
				),
				parsingBlueprintDefinition.InstanceId
			);
		}
	}
}