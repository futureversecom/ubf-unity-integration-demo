// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Native.FFI;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime
{
	/// <summary>
	/// Runtime representation of a UBF Blueprint.
	/// </summary>
	public unsafe class Blueprint
	{
		// well-known root scope id; see execution_context.rs
		private const uint RootScope = 0;

		private readonly GraphInstance* _nativePtr;
		private readonly Dictionary<string, object> _variables = new();
		internal readonly string InstanceId;
		
		internal void RegisterVariable(string name, object value)
		{
			// TODO: Check against blueprint variables
			_variables[name] = value;
		}

		private List<BindingInfo> _cachedInputs;

		/// <summary>
		/// 
		/// </summary>
		public List<BindingInfo> Inputs
		{
			get
			{
				if (_cachedInputs != null)
				{
					return _cachedInputs;
				}

				var output = new List<BindingInfo>();
				var context = GCHandle.Alloc(output);
				Calls.graph_iter_inputs(_nativePtr, (IntPtr)context, IterateInputsCallback);
				context.Free();
				_cachedInputs = output;
				return _cachedInputs;
			}
		}

		private List<BindingInfo> _cachedOutputs;

		/// <summary>
		/// 
		/// </summary>
		public List<BindingInfo> Outputs
		{
			get
			{
				if (_cachedOutputs != null)
				{
					return _cachedOutputs;
				}

				var output = new List<BindingInfo>();
				var context = GCHandle.Alloc(output);
				Calls.graph_iter_outputs(_nativePtr, (IntPtr)context, IterateOutputsCallback);
				context.Free();
				_cachedOutputs = output;
				return _cachedOutputs;
			}
		}

		[MonoPInvokeCallback(typeof(Calls.graph_iter_outputs_iterator_delegate))]
		private static bool IterateOutputsCallback(
			nint context,
			byte* idUtf8,
			int idLen,
			byte* typeUtf8,
			int typeLen,
			Native.FFI.Dynamic* defaultPtr)
		{
			var @default = new Dynamic(defaultPtr);
			var xs = (List<BindingInfo>)GCHandle.FromIntPtr(context)
				.Target;
			xs.Add(
				new BindingInfo
				{
					Id = Encoding.UTF8.GetString(idUtf8, idLen),
					Type = Encoding.UTF8.GetString(typeUtf8, typeLen),
					Default = @default,
				}
			);
			return true;
		}

		[MonoPInvokeCallback(typeof(Calls.graph_iter_inputs_iterator_delegate))]
		private static bool IterateInputsCallback(
			nint context,
			byte* idUtf8,
			int idLen,
			byte* typeUtf8,
			int typeLen,
			Native.FFI.Dynamic* defaultPtr)
		{
			var @default = new Dynamic(defaultPtr);
			var xs = (List<BindingInfo>)GCHandle.FromIntPtr(context)
				.Target;
			xs.Add(
				new BindingInfo
				{
					Id = Encoding.UTF8.GetString(idUtf8, idLen),
					Type = Encoding.UTF8.GetString(typeUtf8, typeLen),
					Default = @default,
				}
			);
			return true;
		}

		private Version _version;

		private Version Version
		{
			get
			{
				if (_version != null)
				{
					return _version;
				}

				ushort* bytes = null;
				nuint bytesLen;
				if (!Calls.graph_version(_nativePtr, &bytes, &bytesLen))
				{
					return null;
				}

				var versionString = new string((char*)bytes, 0, (int)bytesLen);
				return !Version.TryParse(versionString, out _version) ? null : _version;
			}
		}

		private Blueprint(GraphInstance* nativePtr, string instanceId)
		{
			_nativePtr = nativePtr;
			InstanceId = instanceId;
		}

		~Blueprint()
		{
			Calls.graph_release(_nativePtr);
		}

		public struct BindingInfo
		{
			public string Id;
			public string Type;
			public Dynamic Default;
		}

		/// <summary>
		/// Loads a runtime instance of a UBF Blueprint that can be executed by the UBF plugin.
		/// </summary>
		/// <param name="instanceId">The unique identifier for this blueprint.</param>
		/// <param name="json">The raw UBF Blueprint Json to load into the runtime Blueprint.</param>
		/// <param name="blueprint">The resulting runtime Blueprint object.</param>
		/// <param name="customRegistry">A collection of nodes that should be available to this Blueprint. If null, a Default registry is used.</param>
		/// <returns>Whether the blueprint was successfully loaded</returns>
		public static bool TryLoad(string instanceId, string json, out Blueprint blueprint, Registry customRegistry = null)
		{
			var registry = customRegistry ?? Registry.DefaultRegistry;
			fixed (char* p = json)
			{
				var ptr = Calls.graph_load(
					registry.NativePtr,
					(ushort*)p,
					json.Length,
					UbfLogger.GraphLoadLogCallback
				);
				if (ptr is null)
				{
					blueprint = null;
					return false;
				}

				blueprint = new Blueprint(ptr, instanceId);
				if (!blueprint.Version.IsSupported())
				{
					UbfLogger.LogError($"Cannot load blueprint with unsupported standard version ({blueprint.Version})");
					return false;
				}
				
				return true;
			}
		}

		internal ExecutionContext Execute(
			IExecutionConfig executionConfig,
			string graphLabel = null,
			Action onGraphComplete = null,
			Action<string, uint> onNodeStart = null,
			Action<string, uint> onNodeComplete = null)
		{
			var dynamicInputs = Dynamic.From(_variables);
			var contextData = new ExecutionContext.ContextData(
				InstanceId,
				Version,
				executionConfig,
				onGraphComplete: onGraphComplete,
				onNodeStart: onNodeStart,
				onNodeComplete: onNodeComplete
			);

			fixed (char* p = graphLabel ?? "")
			{
				return new ExecutionContext(
					Calls.graph_execute(
						_nativePtr,
						dynamicInputs.NativePtr,
						Dynamic.Foreign(contextData).NativePtr,
						(ushort*)p, graphLabel?.Length ?? 0,
						on_graph_complete: OnGraphComplete,
						on_node_complete: OnNodeComplete,
						on_node_start: OnNodeStart,
						UbfLogger.GraphExecuteLogCallback
					),
					contextData
				);
			}
		}

		[MonoPInvokeCallback(typeof(Calls.graph_execute_on_node_complete_delegate))]
		private static void OnGraphComplete(Native.FFI.Dynamic* userDataPtr)
		{
			var userDataRaw = new Dynamic(userDataPtr);
			if (userDataRaw.TryDeref<ExecutionContext.ContextData>(out var ctxUserData))
			{
				// we must try/catch here as throwing in native callbacks is undefined behavior
				try
				{
					ctxUserData.OnGraphComplete.Invoke();
				}
				catch (Exception e)
				{
					UbfLogger.LogError("Exception in OnGraphComplete callback: " + e);
				}
			}
			else
			{
				// TODO this should never happen; report this.
				UbfLogger.LogError(
					"Failed to deref user data from graph execution context. OnGraphComplete callback will not be called."
				);
			}
		}

		[MonoPInvokeCallback(typeof(Calls.graph_execute_on_node_complete_delegate))]
		private static void OnNodeComplete(
			byte* nodeIdUtf8,
			int nodeIdLen,
			uint scopeId,
			Native.FFI.Dynamic* userDataPtr)
		{
			var userDataRaw = new Dynamic(userDataPtr);
			if (userDataRaw.TryDeref<ExecutionContext.ContextData>(out var ctxUserData))
			{
				// we must try/catch here as throwing in native callbacks is undefined behavior
				try
				{
					var nodeId = Encoding.UTF8.GetString(nodeIdUtf8, nodeIdLen);
					ctxUserData.OnNodeComplete.Invoke(nodeId, scopeId);
				}
				catch (Exception e)
				{
					UbfLogger.LogError("Exception in OnNodeComplete callback: " + e);
				}
			}
			else
			{
				// TODO this should never happen; report this.
				UbfLogger.LogError(
					"Failed to deref user data from graph execution context. OnNodeComplete callback will not be called."
				);
			}
		}

		[MonoPInvokeCallback(typeof(Calls.graph_execute_on_node_start_delegate))]
		private static void OnNodeStart(
			byte* nodeIdUtf8,
			int nodeIdLen,
			uint scopeId,
			Native.FFI.Dynamic* userDataPtr)
		{
			var userDataRaw = new Dynamic(userDataPtr);
			if (userDataRaw.TryDeref<ExecutionContext.ContextData>(out var ctxUserData))
			{
				ctxUserData.PendingScopeIDs.Remove(scopeId);

				// we must try/catch here as throwing in native callbacks is undefined behavior
				try
				{
					var nodeId = Encoding.UTF8.GetString(nodeIdUtf8, nodeIdLen);
					ctxUserData.OnNodeStart.Invoke(nodeId, scopeId);
				}
				catch (Exception e)
				{
					UbfLogger.LogError("Exception in OnNodeStart callback: " + e);
				}
			}
			else
			{
				// TODO this should never happen; report this.
				UbfLogger.LogError(
					"Failed to deref user data from graph execution context. OnNodeStart callback will not be called."
				);
			}
		}
	}

	public class BlueprintExecutionTask : CustomYieldInstruction
	{
		public BlueprintExecutionTask(Blueprint blueprint, IExecutionConfig executionConfig, string graphLabel = null)
		{
			ExecutionContext = blueprint.Execute(
				executionConfig,
				graphLabel: graphLabel,
				onGraphComplete: () => { _isDone = true; }
			);
		}

		private bool _isDone;
		public override bool keepWaiting => !_isDone;

		public ExecutionContext ExecutionContext { get; private set; }
	}
}