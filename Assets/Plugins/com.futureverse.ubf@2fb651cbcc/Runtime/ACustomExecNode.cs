// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime
{
	public abstract class ACustomExecNode : ACustomNode
	{
		protected ACustomExecNode(Context context) : base(context) { }

		private bool _doTrigger = true;
		private string _triggerName = "Exec";
		
		/// <summary>
		/// Trigger another node and wait for it to be done.
		/// </summary>
		/// <param name="key">The name of the Exec output that leads to the next node.</param>
		/// <returns>An IEnumerator to yield on.</returns>
		/// <exception cref="Exception">Node at the specified Exec output was not successfully triggered.</exception>
		protected IEnumerable TriggerAndWait(string key)
		{
			if (NodeContext.ExecutionContext.TriggerNode(
				NodeContext.NodeId,
				key,
				NodeContext.ScopeId,
				out var scopeId
			))
			{
				if (!NodeContext.IsPending)
				{
					yield break;
				}

				yield return new WaitUntil(() => !NodeContext.IsPending);
			}
			else
			{
				// TODO how to deal with errors?
				throw new Exception($"Failed to trigger node {NodeContext.NodeId} with key {key}");
			}
		}
		
		private bool Trigger(string key)
			=> NodeContext.ExecutionContext.TriggerNode(
				NodeContext.NodeId,
				key,
				NodeContext.ScopeId,
				out var _
			);

		/// <summary>
		/// Call this if execution should not continue past this node.
		/// </summary>
		protected void StopExecution()
		{
			_doTrigger = false;
		}

		/// <summary>
		/// Override the default execution trigger "Exec" if an exec pin of this node is named differently.
		/// </summary>
		/// <param name="triggerName">The new trigger name</param>
		protected void SetExecutionTrigger(string triggerName)
		{
			_triggerName = triggerName;
		}
		
		protected internal override void PostExecute()
		{
			if (_doTrigger)
			{
				Trigger(_triggerName);
			}
		}
	}
}