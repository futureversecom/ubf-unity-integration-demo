// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class TransformPositionNode : ACustomExecNode
	{
		public TransformPositionNode(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<Transform>("Transform Object", out var transformObject))
			{
				UbfLogger.LogError("[TransformPositionNode] Could not find input \"Transform Object\"");
				return;
			}

			var useWorldSpace = TryRead<bool>("Use World Space", out var outUseWorldSpace) && outUseWorldSpace;
			var isAdditive = TryRead<bool>("Is Additive", out var outIsAdditive) && outIsAdditive;
			var x = TryRead<float>("Right", out var outX) ? outX : 0;
			var y = TryRead<float>("Up", out var outY) ? outY : 0;
			var z = TryRead<float>("Forward", out var outZ) ? outZ : 0;

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
		}
	}
}