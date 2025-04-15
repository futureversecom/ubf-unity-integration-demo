// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Futureverse.UBF.Runtime
{
	/// <summary>
	///     Custom PlayerLoop system for UBF.
	///     TODO: Consider a UniTask adapter as an alternative, or perhaps
	///     piggy-backing off of Unity's 'MonoBehaviour' to get more
	///     support for more powerful enumerators.
	/// </summary>
	internal static class UBFPlayerLoopSystem
	{
		[RuntimeInitializeOnLoadMethod]
		private static void Initialize()
		{
			PlayerLoopInterface.InsertSystemBefore(
				typeof(UBFPlayerLoopSystem),
				Update,
				typeof(Update.ScriptRunBehaviourUpdate)
			);
		}

		public class CoroutineStatus
		{
			public bool IsDone { get; private set; }

			public void Done()
			{
				IsDone = true;
			}
		}

		private static readonly List<Task> s_tasks = new();

		private struct Task
		{
			public CoroutineStatus Status;
			public ExecutionContext Context;
			public uint ScopeId;
		}

		private static void Update()
		{
			// tick all tasks back to front; remove done tasks
			for (var i = s_tasks.Count - 1; i >= 0; i--)
			{
				var task = s_tasks[i];
				if (!task.Status.IsDone)
				{
					continue;
				}

				task.Context.Complete(task.ScopeId);
				s_tasks.RemoveAt(i);
			}
		}

		internal static void ExecuteNode(
			ACustomNode node,
			string nodeId,
			uint scopeId,
			ExecutionContext context)
		{
			// TODO invoking this here means we are executing the node
			//      at whatever time 'ExecuteNode' was called. this is
			//      important for data nodes to function properly
			//      as they can be called upon at any time to evaluate.
			var status = new CoroutineStatus();
			node.Execute(status);
			if (status.IsDone)
			{
				context.Complete(scopeId);
				return;
			}

			s_tasks.Add(
				new Task
				{
					Status = status,
					Context = context,
					ScopeId = scopeId,
				}
			);
		}
	}
}