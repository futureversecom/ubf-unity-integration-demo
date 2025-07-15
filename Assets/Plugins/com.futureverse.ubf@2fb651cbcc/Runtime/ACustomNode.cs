// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime
{
	/// <summary>
	/// Base class for all nodes implemented in-engine. You can derive this to create your own custom nodes.
	/// </summary>
	public abstract class ACustomNode
	{
		/// <summary>
		/// Contains data relevant for this node's execution.
		/// </summary>
		public readonly struct Context
		{
			internal readonly string NodeId;
			internal readonly uint ScopeId;
			/// <summary>
			/// Access the Context of the Blueprint that is executing this node.
			/// </summary>
			public readonly ExecutionContext ExecutionContext;
			internal bool IsPending => ExecutionContext.IsScopePending(ScopeId);

			internal Context(string nodeId, uint scopeId, ExecutionContext executionContext)
			{
				NodeId = nodeId;
				ScopeId = scopeId;
				ExecutionContext = executionContext;
			}
		}
		
		protected readonly Context NodeContext;

		protected ACustomNode(Context context)
		{
			NodeContext = context;
		}

		/// <summary>
		/// Attempt to retrieve an input from this node.
		/// </summary>
		/// <param name="key">The name of the input to retrieve.</param>
		/// <param name="value">The retrieved value of the input.</param>
		/// <returns>Whether the input was retrieved successfully.</returns>
		protected bool TryRead(string key, out Dynamic value)
			=> NodeContext.ExecutionContext.TryReadInput(NodeContext.NodeId, key, NodeContext.ScopeId, out value);

		/// <summary>
		/// Attempt to retrieve an input from this node and interpret the Dynamic data as a specific type.
		/// </summary>
		/// <param name="key">The name of the input to retrieve.</param>
		/// <param name="value">The retrieved value of the input.</param>
		/// <typeparam name="T">The type to try to cast the input's dynamic data to.</typeparam>
		/// <returns>Whether the input was retrieved successfully.</returns>
		protected bool TryRead<T>(string key, out T value)
			=> NodeContext.ExecutionContext.TryReadInput(NodeContext.NodeId, key, NodeContext.ScopeId, out value);

		/// <summary>
		/// Attempt to retrieve an array input from this node and interpret the inner dynamic data as a specific type.
		/// </summary>
		/// <param name="key">The name of the input to retrieve.</param>
		/// <param name="value">The retrieved value of the array input as a List.</param>
		/// <typeparam name="T">The inner type to try to cast the array's dynamic data to.</typeparam>
		/// <returns>Whether the input was retrieved successfully.</returns>
		protected bool TryReadArray<T>(string key, out List<T> value)
			=> NodeContext.ExecutionContext.TryReadArrayInput(NodeContext.NodeId, key, NodeContext.ScopeId, out value);

		/// <summary>
		/// Assign an output for this node.
		/// </summary>
		/// <param name="key">The name of the output to assign.</param>
		/// <param name="value">The value to assign.</param>
		protected void WriteOutput(string key, object value)
		{
			NodeContext.ExecutionContext.WriteOutput(NodeContext.NodeId, key, value);
		}

		/// <summary>
		/// Retrieve an input of a Resource type, and create a ResourceId instance to wrap it.
		/// </summary>
		/// <param name="pinName">The name of the input to read.</param>
		/// <param name="resourceId">The resulting resource ID wrapper.</param>
		/// <returns>Whether the resource ID was retrieved successfully.</returns>
		protected bool TryReadResourceId(string pinName, out ResourceId resourceId)
		{
			if (TryRead(pinName, out string raw))
			{
				resourceId = ResourceId.UnsafeFromString(raw);
				return true;
			}

			resourceId = default(ResourceId);
			return false;
		}

		/// Use a private custom exception to use for special control flow.
		private class ExecuteNotImplementedException : NotImplementedException { }

		internal void Execute(UBFPlayerLoopSystem.CoroutineStatus status)
		{
			try
			{
				ExecuteSync();
				PostExecute();
				status.Done();
			}
			catch (ExecuteNotImplementedException)
			{
				CoroutineHost.Instance.StartCoroutine(ExecuteAsyncWrapper(status));
			}
		}

		private IEnumerator ExecuteAsyncWrapper(UBFPlayerLoopSystem.CoroutineStatus status)
		{
			var routine = CoroutineHost.Instance.StartCoroutine(ExecuteAsync());
			if (routine != null)
			{
				yield return routine;
			}

			PostExecute();
			status.Done();
		}

		protected internal virtual void PostExecute()
		{
			return;
		}

		/// <summary>
		/// Override this to implement a node that executes synchronously, i.e. in one frame
		/// </summary>
		protected virtual void ExecuteSync()
		{
			throw new ExecuteNotImplementedException();
		}

		/// <summary>
		/// Override this to implement a node that executes asynchronously, and can yield/wait for other processes
		/// </summary>
		/// <returns>An IEnumerator to yield on.</returns>
		protected virtual IEnumerator ExecuteAsync()
		{
			yield break;
		}
	}
}