// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class SetBlendShape : ACustomExecNode
	{
		public SetBlendShape(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<MeshRendererSceneComponent>("Target", out var rendererComponent) || rendererComponent == null)
			{
				UbfLogger.LogError("[SetBlendShape] Could not find input \"Target\"");
				return;
			}
			
			if (!TryRead<string>("ID", out var blendShapeId))
			{
				UbfLogger.LogError("[SetBlendShape] Could not find input \"ID\"");
				return;
			}
			
			if (!TryRead<float>("Value", out var blendShapeValue))
			{
				UbfLogger.LogError("[SetBlendShape] Could not find input \"Value\"");
				return;
			}

			if (!rendererComponent.skinned)
			{
				UbfLogger.LogError("[SetBlendShape] Target renderer is not skinned");
				return;
			}

			foreach (var mRender in rendererComponent.TargetMeshRenderers)
			{
				var smr = mRender as SkinnedMeshRenderer;
				if (smr == null)
				{
					UbfLogger.LogError("[SetBlendShape] Mesh Renderer component is not skinned");
					continue;
				}

				var index = smr.sharedMesh.GetBlendShapeIndex(blendShapeId);
				if (index == -1)
				{
					UbfLogger.LogError($"[SetBlendShape] Cannot find blend-shape on renderer. Blend ID: {blendShapeId}");
					continue;
				}

				smr.SetBlendShapeWeight(index, blendShapeValue);
			}
			
		}
	}
}