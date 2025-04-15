// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class CreateSceneNode : ACustomNode
	{
		public CreateSceneNode(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			var nodeName = "NewSceneNode";
			if (TryRead<string>("Name", out var name) && !string.IsNullOrEmpty(name))
			{
				nodeName = name;
			}

			var gameObject = new GameObject(nodeName);

			if (TryRead<Transform>("Parent", out var parent))
			{
				gameObject.transform.SetParent(parent);
			}
			else
			{
				gameObject.transform.SetParent(NodeContext.ExecutionContext.Config.GetRootTransform);
			}

			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;

			WriteOutput("Node", Dynamic.Foreign(gameObject.transform));
			TriggerNext();
		}
	}
}