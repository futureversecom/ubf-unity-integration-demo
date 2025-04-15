// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class FindSceneNodes : ACustomNode
	{
		public FindSceneNodes(Context context) : base(context) { }

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

			var dynamicArray = Dynamic.Array();

			FindNodes(filter, rootTransform, node => { dynamicArray.Push(Dynamic.Foreign(node)); });

			WriteOutput("Nodes", dynamicArray);
		}

		private static void FindNodes(string filter, Transform root, Action<Transform> action)
		{
			foreach (Transform child in root)
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