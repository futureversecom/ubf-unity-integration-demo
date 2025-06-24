// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using AssetRegister.Runtime.Schema.Objects;
using AssetRegister.Runtime.Schema.Unions;
using Futureverse.UBF.Runtime.Execution;
using UnityEngine;

namespace Futureverse.UBF.UBFExecutionController.Runtime
{
	/// <summary>
	/// Responsible for turning an Asset object from Asset Register into the data required for UBF. Create a subclass
	/// of this if you need to override this behaviour, e.g. you want to use a custom Artifact Provider.
	/// </summary>
	public interface IAssetParser
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="loadedAssets">All the loaded Assets from Asset Register that are used to fill out the Asset Tree.</param>
		/// <param name="assetId">The Asset ID of the root Asset</param>
		/// <param name="callback">A tuple containing a list of blueprint instance data, the artifact provider, and the instance ID of the root blueprint</param>
		/// <returns></returns>
		IEnumerator ParseAsset(Dictionary<string, Asset> loadedAssets, string assetId, Action<(List<IBlueprintInstanceData>, IArtifactProvider, string)> callback);
	}

	public class DefaultAssetParser : IAssetParser
	{
		public IEnumerator ParseAsset(
			Dictionary<string, Asset> loadedAssets,
			string assetId,
			Action<(List<IBlueprintInstanceData>, IArtifactProvider, string)> callback)
		{
			var artifactProvider = new ArtifactProvider();
			var blueprints = new List<IBlueprintInstanceData>();

			string rootId = null;
			yield return PrepareAssetRecursive(
				loadedAssets,
				assetId,
				blueprints,
				artifactProvider,
				i => rootId = i.InstanceId
			);
			
			callback?.Invoke((blueprints, artifactProvider, rootId));
		}
		
		private static IEnumerator PrepareAssetRecursive(
			Dictionary<string, Asset> loadedAssets,
			string assetId,
			List<IBlueprintInstanceData> blueprintInstances,
			ArtifactProvider artifactProvider,
			Action<IBlueprintInstanceData> callback)
		{
			if (!loadedAssets.TryGetValue(assetId, out var asset))
			{
				Debug.LogWarning("Unexpected asset requested - Asset is not loaded");
				yield break;
			}

			AssetProfile profile = null;
			yield return AssetProfile.FetchByAsset(
				asset,
				p => profile = p
			);
			
			if (profile == null)
			{
				Debug.LogError("Failed to fetch asset profile");
				yield break;
			}
			
			if (profile.RenderCatalog == null)
			{
				Debug.LogError("Render catalog is null!");
				yield break;
			}
			
			artifactProvider.RegisterCatalog(profile.RenderCatalog);
			var renderBlueprintDefinition = new BlueprintInstanceData(profile.RenderBlueprintResourceId);
			
			yield return GetParsingInputs(profile, asset.GetFullMetadata(), renderBlueprintDefinition);

			if (asset.Links is NFTAssetLink nftLink)
			{
				foreach (var link in nftLink.ChildLinks)
				{
					var path = link.Path.Split("#")[^1]
						.Replace("_accessory", "");

					yield return PrepareAssetRecursive(
						loadedAssets,
						link.Asset.Id,
						blueprintInstances,
						artifactProvider,
						i => renderBlueprintDefinition.AddInput(path, i.InstanceId)
					);
				}
			}
			
			blueprintInstances.Add(renderBlueprintDefinition);
			callback?.Invoke(renderBlueprintDefinition);
		}

		private static IEnumerator GetParsingInputs(AssetProfile profile, string metadata, BlueprintInstanceData renderBlueprint)
		{
			if (profile.ParsingCatalog == null || string.IsNullOrEmpty(profile.ParsingBlueprintResourceId))
			{
				yield break;
			}
			
			var parsingBlueprintDefinition = new BlueprintInstanceData(profile.ParsingBlueprintResourceId);
			parsingBlueprintDefinition.AddInput("metadata", metadata);
			
			var parsingArtifactProvider = new ArtifactProvider();
			parsingArtifactProvider.RegisterCatalog(profile.ParsingCatalog);

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
					},
					parsingArtifactProvider
				),
				parsingBlueprintDefinition.InstanceId
			);
		}
	}
}