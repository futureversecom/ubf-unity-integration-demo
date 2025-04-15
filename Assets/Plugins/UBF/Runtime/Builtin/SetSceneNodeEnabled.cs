// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class SetSceneNodeEnabled : ACustomNode
	{
		public SetSceneNodeEnabled(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<Transform>("Node", out var node))
			{
				Debug.LogWarning("Failed to read node");
				TriggerNext();
				return;
			}

			if (!TryRead<bool>("Enabled", out var enabled))
			{
				Debug.LogWarning("Failed to read enabled state");
				TriggerNext();
				return;
			}

			node.gameObject.SetActive(enabled);
			TriggerNext();
		}
	}
}