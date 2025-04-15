// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class FindFirstSceneNode : ACustomNode
	{
		public FindFirstSceneNode(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<Transform>("Root", out var rootTransform))
			{
				Debug.LogWarning("Find Scene Nodes node has no input 'Root'");
				return;
			}

			if (!TryRead<string>("Filter", out var filter))
			{
				Debug.LogWarning("Find Scene Nodes node could not find input 'Filter'");
				return;
			}

			var list = new List<Transform>();
			FindNodes(filter, rootTransform, node => { list.Add(node); });

			if (list.Count == 0)
			{
				Debug.LogWarning($"No scene node found with filter {filter}");
				return;
			}

			WriteOutput("Node", Dynamic.Foreign(list[0]));
		}

		private static void FindNodes(string filter, Transform root, Action<Transform> action)
		{
			foreach (Transform child in root.transform)
			{
				// TODO: support more comprehensive filtering (perhaps a glob pattern?)
				if (child.name.StartsWith(filter))
				{
					action(child);
				}

				FindNodes(filter, child, action);
			}
		}
	}
}