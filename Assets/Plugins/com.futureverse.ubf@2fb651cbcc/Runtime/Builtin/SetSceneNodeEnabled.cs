// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using Futureverse.UBF.Runtime.Utils;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class SetSceneNodeEnabled : ACustomExecNode
	{
		public SetSceneNodeEnabled(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<SceneNode>("SceneNode", out var node))
			{
				UbfLogger.LogError("[SetSceneNodeEnabled] Could not find input \"Node\"");
				return;
			}

			if (!TryRead<bool>("Enabled", out var enabled))
			{
				UbfLogger.LogError("[SetSceneNodeEnabled] Could not find input \"Enabled\"");
				return;
			}

			node.TargetSceneObject.gameObject.SetActive(enabled);
		}
	}
}