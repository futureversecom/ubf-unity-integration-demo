// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Native.FFI;
using UnityEngine;

namespace Futureverse.UBF.Runtime
{
	/// <summary>
	/// Runtime representation of a UBF Blueprint.
	/// </summary>
	public unsafe class Blueprint
	{
		private readonly GraphInstance* _nativePtr;
		private readonly Dictionary<string, object> _variables = new();
		internal readonly string InstanceId;
		
		internal void RegisterVariable(string name, object value)
		{
			// TODO: Check against blueprint variables
			_variables[name] = value;
		}

		private List<BindingInfo> _cachedInputs;

		internal List<BindingInfo> Inputs
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

		internal List<BindingInfo> Outputs
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
		private bool IterateOutputsCallback(
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
		private bool IterateInputsCallback(
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

		private BlueprintVersion _version;

		internal BlueprintVersion Version
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
				_version = BlueprintVersion.FromString(versionString);

				return _version;
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

		internal struct BindingInfo
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
			if (!GraphVersionUtils.JsonHasSupportedVersion(json))
			{
				Debug.LogError("Cannot load blueprint with unsupported UBF Standard version");
				blueprint = null;
				return false;
			}

			fixed (char* p = json)
			{
				var ptr = Calls.graph_load(registry.NativePtr, (ushort*)p, json.Length);
				if (ptr is null)
				{
					blueprint = null;
					return false;
				}

				blueprint = new Blueprint(ptr, instanceId);
				return true;
			}
		}

		internal ExecutionContext Execute(IExecutionConfig executionConfig, Action onComplete = null)
		{
			var dynamicInputs = Dynamic.From(_variables);
			var contextData = new ExecutionContext.ContextData(InstanceId, executionConfig, onComplete);

			return new ExecutionContext(
				Calls.graph_execute(
					_nativePtr,
					dynamicInputs.NativePtr,
					Dynamic.Foreign(contextData)
						.NativePtr,
					OnNodeComplete
				),
				contextData
			);
		}

		[MonoPInvokeCallback(typeof(Calls.graph_execute_on_node_complete_delegate))]
		private static void OnNodeComplete(Native.FFI.Dynamic* userDataPtr, uint scope)
		{
			var userDataRaw = new Dynamic(userDataPtr);
			if (userDataRaw.TryDeref<ExecutionContext.ContextData>(out var ctxUserData))
			{
				ctxUserData.PendingScopeIDs.Remove(scope);

				// we must try/catch here as throwing in native callbacks is undefined behavior
				try
				{
					// TODO make this more robust (0 = initial scope = entry scope.)
					// perhaps the rust interpreter should fire off a special event for
					// this so that 0 can remain an implementation detail!
					if (scope == 0)
					{
						ctxUserData.OnComplete.Invoke();
					}
				}
				catch (Exception e)
				{
					Debug.LogError("Exception in OnComplete callback: " + e);
				}
			}
			else
			{
				// TODO this should never happen; report this.
				Debug.LogError(
					"Failed to deref user data from graph execution context. OnComplete callback will not be called."
				);
			}
		}
	}

	public class BlueprintExecutionTask : CustomYieldInstruction
	{
		public BlueprintExecutionTask(Blueprint blueprint, IExecutionConfig executionConfig)
		{
			ExecutionContext = blueprint.Execute(executionConfig, () => { _isDone = true; });
		}

		private bool _isDone;
		public override bool keepWaiting => !_isDone;

		public ExecutionContext ExecutionContext { get; private set; }
	}
}