// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class TransformScaleNode : ACustomNode
	{
		public TransformScaleNode(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<Transform>("Transform Object", out var transformObject))
			{
				Debug.LogError("No transform supplied to TransformScaleNode");
				TriggerNext();
				return;
			}

			if (!TryRead<bool>("Is Additive", out var isAdditive))
			{
				Debug.LogError("No option supplied for Is Additive input");
				TriggerNext();
				return;
			}

			if (!TryRead<float>("Right", out var x))
			{
				x = 0;
			}

			if (!TryRead<float>("Up", out var y))
			{
				y = 0;
			}

			if (!TryRead<float>("Forward", out var z))
			{
				z = 0;
			}

			if (isAdditive)
			{
				transformObject.localScale += new Vector3(x, y, z);
			}
			else
			{
				transformObject.localScale = new Vector3(x, y, z);
			}

			TriggerNext();
		}
	}
}