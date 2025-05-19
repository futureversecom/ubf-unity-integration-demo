// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Resources;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Futureverse.UBF.ExecutionController.Runtime
{
	public class AssetProfileDataParser : IUbfDataParser
	{
		private readonly string[] _variantOverrides;

		public AssetProfileDataParser(string[] variantOverrides)
		{
			_variantOverrides = variantOverrides;
		}

		public AssetProfileDataParser()
		{
			
		}
		
		public IEnumerator GetBlueprintDefinition(IUbfData data, Action<IBlueprintInstanceData, Catalog> callback)
		{
			if (data is not IUbfAsset asset)
			{
				Debug.LogError($"UbfAsset not found with AssetId {data.Id}");
				callback?.Invoke(null, null);
				yield break;
			}

			AssetProfile assetProfile = null;
			yield return AssetProfile.FetchByAssetId(
				asset.CollectionId,
				asset.TokenId,
				profile => assetProfile = profile,
				_variantOverrides
			);
			if (assetProfile == null)
			{
				Debug.LogError($"Couldn't get asset profile at {asset.Id}");
				callback?.Invoke(null, null);
				yield break;
			}

			var renderBlueprintDefinition = new BlueprintInstanceData(assetProfile.RenderBlueprintResourceId);
			if (string.IsNullOrEmpty(assetProfile.ParsingBlueprintResourceId))
			{
				callback?.Invoke(renderBlueprintDefinition, assetProfile.RenderCatalog);
				yield break;
			}

			var parsingBlueprintDefinition = new BlueprintInstanceData(assetProfile.ParsingBlueprintResourceId);

			var meta = asset.Metadata;
			if (meta.TryGetValue("metadata", out var unwrappedToken))
			{
				parsingBlueprintDefinition.AddInput("metadata", unwrappedToken.ToString());
			}
			else
			{
				parsingBlueprintDefinition.AddInput("metadata", meta.ToString());
			}
			var parsingArtifactProvider = new ArtifactProvider(null);
			parsingArtifactProvider.RegisterCatalog(assetProfile.ParsingCatalog);

			yield return UBFExecutor.ExecuteRoutine(
				new ExecutionData(
					null,
					result =>
					{
						foreach (var output in result.BlueprintOutputs)
						{
							renderBlueprintDefinition.AddInput(output.Key, output.Value);
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

			callback?.Invoke(renderBlueprintDefinition, assetProfile.RenderCatalog);
		}
	}
}