// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class TransformScaleNode : ACustomExecNode
	{
		public TransformScaleNode(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<SceneNode>("Transform Object", out var transformObject))
			{
				UbfLogger.LogError("[TransformScaleNode] Could not find input \"Transform Object\"");
				return;
			}
			
			var isAdditive = TryRead<bool>("Is Additive", out var outIsAdditive) && outIsAdditive;
			var x = TryRead<float>("Right", out var outX) ? outX : 0;
			var y = TryRead<float>("Up", out var outY) ? outY : 0;
			var z = TryRead<float>("Forward", out var outZ) ? outZ : 0;

			if (isAdditive)
			{
				transformObject.TargetSceneObject.transform.localScale += new Vector3(x, y, z);
			}
			else
			{
				transformObject.TargetSceneObject.transform.localScale = new Vector3(x, y, z);
			}
		}
	}
}