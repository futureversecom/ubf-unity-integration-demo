// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class ApplyMaterial : ACustomExecNode
	{
		public ApplyMaterial(Context context) : base(context) { }

		protected override IEnumerator ExecuteAsync()
		{
			if (!TryRead<MaterialValue>("Material", out var materialValue))
			{
				UbfLogger.LogError("[ApplyMaterial] Could not find input \"Material\"");
				yield break;
			}

			if (!TryRead<Renderer>("Renderer", out var renderer))
			{
				UbfLogger.LogError("[ApplyMaterial] Could not find input \"Renderer\"");
				yield break;
			}

			if (!TryRead<int>("Index", out var matIndex))
			{
				matIndex = 0;
			}

			var material = new Material(materialValue.Material);

			foreach (var prop in materialValue.Properties)
			{
				switch (prop.Value)
				{
					case int intValue:
						material.SetFloat(prop.Key, intValue);
						break;
					case bool and true:
						material.EnableKeyword(prop.Key);
						material.SetFloat(prop.Key, 1.0f);
						break;
					case bool:
						material.DisableKeyword(prop.Key);
						material.SetFloat(prop.Key, 0.0f);
						break;
					case float floatValue:
						material.SetFloat(prop.Key, floatValue);
						break;
					case Color colourValue:
						material.SetColor(prop.Key, colourValue);
						break;
					case Vector3 v3Value:
						material.SetVector(prop.Key, v3Value);
						break;
					case Texture2D texValue:
						material.SetTexture(prop.Key, texValue);
						break;
					case ResourceId resourceId:
					{
						TextureImportSettings textureSettings = null;
						if (NodeContext.ExecutionContext.GetDynamicDataEntry(
								resourceId.Value,
								out var dynamic
							) &&
							dynamic.TryInterpretAs<TextureImportSettings>(out var settings))
						{
							textureSettings = settings;
						}

						Texture2D textureResource = null;
						var routine = CoroutineHost.Instance.StartCoroutine(
							NodeContext.ExecutionContext.Config.GetTextureInstance(
								resourceId,
								textureSettings,
								(texture, _) => { textureResource = texture; }
							)
						);
						if (routine != null)
						{
							yield return routine;
						}

						if (textureResource == null)
						{
							UbfLogger.LogError(
								$"[ApplyMaterial] Could not load texture resource with ID \"{resourceId.Value}\""
							);
							continue;
						}

						material.SetTexture(prop.Key, textureResource);
						break;
					}
				}
			}

			var rMats = renderer.materials;
			rMats[matIndex] = material;
			renderer.materials = rMats;
		}
	}
}