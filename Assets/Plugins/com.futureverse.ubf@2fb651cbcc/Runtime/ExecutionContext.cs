// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AOT;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Native.FFI;
using Futureverse.UBF.Runtime.Utils;

[assembly: InternalsVisibleTo("com.futureverse.ubf.tests.utils")]
[assembly: InternalsVisibleTo("com.futureverse.ubf.tests.0.2.0")]

namespace Futureverse.UBF.Runtime
{
	/// <summary>
	/// Contains data about the execution of a UBF Blueprint. This is the interface through which to get Blueprint
	/// inputs, outputs, status, user data, etc.
	/// </summary>
	public unsafe class ExecutionContext
	{
		private readonly ContextData _contextData;

		private ArcExecutionContext* NativePtr { get; }

		/// <summary>
		/// Retrieve the ExecutionConfig that was created by the ExecutionData. All data here is passed down to
		/// child graphs as well.
		/// </summary>
		public IExecutionConfig Config => _contextData.ExecutionConfig;
		/// <summary>
		/// 
		/// </summary>
		public Version BlueprintVersion => _contextData.BlueprintVersion;
		/// <summary>
		/// The unique identifier for this Blueprint.
		/// </summary>
		public string InstanceId => _contextData.InstanceId;

		/// <summary>
		///
		/// </summary>
		/// <param name="version">Semantic version string</param>
		/// <returns>If the UBF Standard version the running Blueprint was created against is greater or equal to the provided version</returns>
		public bool BlueprintVersionIsGreaterOrEqualTo(string version)
		{
			if (!Version.TryParse(version, out var v))
			{
				UbfLogger.LogWarn("Cannot compare invalid Blueprint version");
				return false;
			}
			
			return _contextData.BlueprintVersion >= v;
		}
		
		/// <summary>
		///
		/// </summary>
		/// <param name="version">Semantic version string</param>
		/// <returns>If the UBF Standard version the running Blueprint was created against is less than the provided version</returns>
		public bool BlueprintVersionIsLessThan(string version)
		{
			if (!Version.TryParse(version, out var v))
			{
				UbfLogger.LogWarn("Cannot compare invalid Blueprint version");
				return false;
			}
			
			return _contextData.BlueprintVersion < v;
		}

		internal bool IsScopePending(uint scopeId)
			=> _contextData.PendingScopeIDs.Contains(scopeId);

		~ExecutionContext()
		{
			Calls.ctx_release(NativePtr);
		}

		internal class ContextData
		{
			public readonly string InstanceId;
			public readonly Version BlueprintVersion;
			public HashSet<uint> PendingScopeIDs { get; } = new();
			public readonly IExecutionConfig ExecutionConfig;

			public readonly Action OnGraphComplete;
			public readonly Action</* node id: */ string, /* scope id: */ uint> OnNodeStart;
			public readonly Action</* node id: */ string,  /* scope id: */ uint> OnNodeComplete;

			public ContextData(
				string instanceId,
				Version blueprintVersion,
				IExecutionConfig executionConfig,
				Action onGraphComplete,
				Action<string, uint> onNodeStart = null,
				Action<string, uint> onNodeComplete = null
			)
			{
				InstanceId = instanceId;
				BlueprintVersion = blueprintVersion;
				ExecutionConfig = executionConfig;
				OnGraphComplete = onGraphComplete;
				OnNodeStart = onNodeStart ?? ((_, _) => { });
				OnNodeComplete = onNodeComplete ?? ((_, _) => { });
			}
		}

		internal ExecutionContext(ArcExecutionContext* ptr, ContextData contextData)
		{
			NativePtr = ptr;
			_contextData = contextData;
		}

		internal void Complete(uint scopeId)
		{
			Calls.ctx_complete_node(NativePtr, scopeId);
		}
		
		internal bool TryReadInput<T>(string nodeId, string portKey, uint scope, out T value)
		{
			if (TryReadInput(nodeId, portKey, scope, out var dynamic))
			{
				return dynamic.TryInterpretAs(out value);
			}

			value = default(T);
			return false;
		}
		
		internal bool TryReadArrayInput<T>(string nodeId, string portKey, uint scope, out List<T> value)
		{
			if (TryReadInput(nodeId, portKey, scope, out var dynamic))
			{
				return dynamic.TryReadArray(out value);
			}

			value = null;
			return false;
		}
		
		internal bool TryReadOutput(string bindingId, out Dynamic value)
		{
			fixed (char* bindingIdUtf16 = bindingId)
			{
				Native.FFI.Dynamic* dynamic;
				if (Calls.ctx_read_output(
					NativePtr,
					(ushort*)bindingIdUtf16,
					bindingId.Length,
					&dynamic
				))
				{
					value = new Dynamic(dynamic);
					return true;
				}
			}

			value = null;
			return false;
		}

		internal bool TryReadInput(string nodeId, string portKey, uint scope, out Dynamic value)
		{
			fixed (char* nodeIdUtf16 = nodeId, portKeyUtf16 = portKey)
			{
				Native.FFI.Dynamic* dynamic;
				if (Calls.ctx_read_input(
					NativePtr,
					(ushort*)nodeIdUtf16,
					nodeId.Length,
					(ushort*)portKeyUtf16,
					portKey.Length,
					scope,
					&dynamic
				))
				{
					value = new Dynamic(dynamic);
					return true;
				}
			}

			value = null;
			return false;
		}

		internal void WriteOutput(string nodeId, string portKey, object value)
		{
			var dyn = Dynamic.From(value);
			fixed (char* nodeIdBytes = nodeId, portKeyBytes = portKey)
			{
				Calls.ctx_write_output(
					NativePtr,
					(ushort*)nodeIdBytes,
					nodeId.Length,
					(ushort*)portKeyBytes,
					portKey.Length,
					dyn.NativePtr
				);
			}
		}

		internal List<string> GetDeclaredNodeInputs(string nodeId)
		{
			fixed (char* nodeIdBytes = nodeId)
			{
				var output = new List<string>();
				var context = GCHandle.Alloc(output);
				Calls.ctx_get_declared_node_inputs(
					NativePtr,
					(ushort*)nodeIdBytes,
					nodeId.Length,
					(IntPtr)context,
					GetDeclaredNodeInputsCallback
				);
				context.Free();
				return output;
			}
		}

		[MonoPInvokeCallback(typeof(Calls.ctx_get_declared_node_inputs_iterator_delegate))]
		private static bool GetDeclaredNodeInputsCallback(nint context, byte* id, int idLen)
		{
			var key = Encoding.UTF8.GetString(id, idLen);
			var outputDict = (List<string>)GCHandle.FromIntPtr(context)
				.Target;
			outputDict.Add(key);
			return true;
		}

		internal bool TriggerNode(
			string sourceNodeID,
			string sourcePortKey,
			uint scope,
			out uint childScope)
		{
			fixed (char* sourceNodeIDBytes = sourceNodeID, sourcePortKeyBytes = sourcePortKey)
			{
				uint cs = 0;
				var ret = Calls.ctx_trigger_node(
					NativePtr,
					(ushort*)sourceNodeIDBytes,
					sourceNodeID.Length,
					(ushort*)sourcePortKeyBytes,
					sourcePortKey.Length,
					scope,
					&cs
				);

				childScope = cs;

				// see rust FFI for details
				switch (ret)
				{
					/* pending */
					case 1:
						_contextData.PendingScopeIDs.Add(cs);
						break;
					/* error */
					case 2:
						return false;
				}

				return true;
			}
		}

		internal Dictionary<string, Dynamic> GetBlueprintOutputs()
		{
			var output = new Dictionary<string, Dynamic>();
			var context = GCHandle.Alloc(output);
			Calls.ctx_get_graph_outputs(NativePtr, (IntPtr)context, IterateGraphOutputsCallback);
			context.Free();
			return output;
		}

		[MonoPInvokeCallback(typeof(Calls.graph_iter_outputs_iterator_delegate))]
		private static bool IterateGraphOutputsCallback(
			nint context,
			byte* id,
			int idLen,
			Native.FFI.Dynamic* valuePtr)
		{
			var key = Encoding.UTF8.GetString(id, idLen);
			var value = new Dynamic(valuePtr);
			var outputDict = (Dictionary<string, Dynamic>)GCHandle.FromIntPtr(context)
				.Target;
			outputDict.Add(key, value);
			return true;
		}

		/// <summary>
		/// Retrieve a piece of arbitrary user data that was stored earlier in the Blueprint's execution.
		/// </summary>
		/// <param name="key">The key under which the data is stored.</param>
		/// <param name="data">The retrieved data.</param>
		/// <returns>Whether the data was successfully retrieved.</returns>
		public bool GetDynamicDataEntry(string key, out Dynamic data)
		{
			fixed (char* keyBytes = key)
			{
				Native.FFI.Dynamic* dynamic;
				if (Calls.ctx_get_dynamic_data_entry(
					NativePtr,
					(ushort*)keyBytes,
					key.Length,
					&dynamic
				))
				{
					data = new Dynamic(dynamic);
					return true;
				}

				data = null;
				return false;
			}
		}

		/// <summary>
		/// Assign an arbitrary piece of user data to the runtime Blueprint. Can be retrieved again any time during the rest of the execution.
		/// </summary>
		/// <param name="key">The key by which to index the data, and retrieve it again.</param>
		/// <param name="data">The data to store.</param>
		/// <returns>Whether the data was successfully stored.</returns>
		public bool SetDynamicDataEntry(string key, Dynamic data)
		{
			fixed (char* keyBytes = key)
			{
				var result = Calls.ctx_set_dynamic_data_entry(
					NativePtr,
					(ushort*)keyBytes,
					key.Length,
					data.NativePtr
				);
				return result;
			}
		}
	}
}