// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class SetBlendshape : ACustomNode
	{
		public SetBlendshape(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<Renderer>("Target", out var targetRenderer) || targetRenderer == null)
			{
				Debug.LogWarning("[SetBlendshape] No Renderer supplied");
				TriggerNext();
				return;
			}

			TryRead<string>("ID", out var blendshapeID);
			TryRead<float>("Value", out var blendshapeValue);

			var smr = targetRenderer as SkinnedMeshRenderer;
			if (smr == null)
			{
				Debug.Log("[SetBlendshape] No Skinned Mesh Renderer on target transform");
				TriggerNext();
				return;
			}

			var index = smr.sharedMesh.GetBlendShapeIndex(blendshapeID);
			if (index == -1)
			{
				Debug.Log("[SetBlendshape] Invalid blendshape ID: " + blendshapeID);
				TriggerNext();
				return;
			}

			smr.SetBlendShapeWeight(index, blendshapeValue);

			TriggerNext();
		}
	}
}