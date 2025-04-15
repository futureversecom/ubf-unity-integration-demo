// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class TransformPositionNode : ACustomNode
	{
		public TransformPositionNode(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<Transform>("Transform Object", out var transformObject))
			{
				Debug.LogError("No transform supplied to TransformPositionNode");
				TriggerNext();
				return;
			}

			if (!TryRead<bool>("Use World Space", out var useWorldSpace))
			{
				Debug.LogError("No option supplied for Use World Space input");
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

			if (useWorldSpace)
			{
				if (isAdditive)
				{
					transformObject.position += new Vector3(x, y, z);
				}
				else
				{
					transformObject.position = new Vector3(x, y, z);
				}
			}
			else
			{
				if (isAdditive)
				{
					transformObject.localPosition += new Vector3(x, y, z);
				}
				else
				{
					transformObject.localPosition = new Vector3(x, y, z);
				}
			}

			TriggerNext();
		}
	}
}