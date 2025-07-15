// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class TransformRotationNode : ACustomExecNode
	{
		public TransformRotationNode(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<Transform>("Transform Object", out var transformObject))
			{
				UbfLogger.LogError("[TransformRotationNode] Could not find input \"Transform Object\"");
				return;
			}

			var useWorldSpace = TryRead<bool>("Use World Space", out var outUseWorldSpace) && outUseWorldSpace;
			var isAdditive = TryRead<bool>("Is Additive", out var outIsAdditive) && outIsAdditive;
			var pitch = TryRead<float>("Pitch", out var outPitch) ? outPitch : 0;
			var yaw = TryRead<float>("Yaw", out var outYaw) ? outYaw : 0;
			var roll = TryRead<float>("Roll", out var outRoll) ? outRoll : 0;

			var rotation = Quaternion.Euler(pitch, yaw, roll);

			if (useWorldSpace)
			{
				if (isAdditive)
				{
					transformObject.rotation = rotation * transformObject.rotation;
				}
				else
				{
					transformObject.rotation = rotation;
				}
			}
			else
			{
				if (isAdditive)
				{
					transformObject.localRotation *= rotation;
				}
				else
				{
					transformObject.localRotation = rotation;
				}
			}
		}
	}
}