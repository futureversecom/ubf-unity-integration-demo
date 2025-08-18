// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class CreateSceneNode : ACustomExecNode
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
			var node = new SceneNode
			{
				TargetSceneObject = gameObject,
				Name = nodeName
			};

			if (TryRead<SceneNode>("Parent", out var parent) && parent != null)
			{
				gameObject.transform.SetParent(parent.TargetSceneObject.transform);
				parent.AddChild(node);
			}
			else
			{
				gameObject.transform.SetParent(NodeContext.ExecutionContext.Config.GetRootTransform);
			}

			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;

			
			
			WriteOutput("Node", node);
		}
	}
}