// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Runtime.InteropServices;
using AOT;
using Futureverse.UBF.Runtime.Builtin;
using Futureverse.UBF.Runtime.Builtin.ResourceLoading;
using Futureverse.UBF.Runtime.Native;
using Futureverse.UBF.Runtime.Native.FFI;
using Plugins.UBF.Runtime.Builtin.ResourceLoading;

namespace Futureverse.UBF.Runtime
{
	/// <summary>
	/// Defines a set of Nodes that are defined in-engine that the Blueprints should be able to use during execution.
	/// </summary>
	public unsafe class Registry
	{
		internal readonly NodeRegistry* NativePtr = Calls.registry_new();

		~Registry()
		{
			Calls.registry_release(NativePtr);
		}

		/// <summary>
		/// Create a registry that contains all the UBF nodes defined in the UBF package.
		/// </summary>
		public static Registry DefaultRegistry
		{
			get
			{
				s_defaultRegistry ??= MakeDefault();
				return s_defaultRegistry;
			}
		}

		private static Registry s_defaultRegistry;

		private static Registry MakeDefault()
			=> new Registry()
				.Register<DebugLog>()
				.Register<FindSceneNodes>()
				.Register<SpawnMesh>()
				.Register<SpawnModel>()
				.Register<SpawnModelWithLods>("SpawnModelWithLODs")
				.Register<BindMeshes>()
				.Register<ApplyMaterial>()
				.Register<SetBlendShape>("SetBlendshape")
				.Register<SetTextureSettings>()
				.Register<CreateSceneNode>()
				.Register<CreateMeshConfig>()
				.Register<FindRenderer>()
				.Register<SetSceneNodeEnabled>()
				.Register<ExecuteBlueprint>()
				.Register<ExecuteBlueprint>("ExecuteBlueprint2")
				.Register<TransformPositionNode>("TransformPosition") // Class includes 'node' due to potential conflicts re Transform
				.Register<TransformRotationNode>("TransformRotation")
				.Register<TransformScaleNode>("TransformScale")
				.Register<MakePBRMaterial>()
				.Register<MakeDecalMaterial>()
				.Register<MakeFurMaterial>()
				.Register<MakeHairMaterial>()
				.Register<MakeSkinMaterial>()
				.Register<MakeSkin02Material>()
				.Register<CreateTextureResource>()
				.Register<CreateBlueprintResource>()
				.Register<CreateMeshResource>()
				.Register<CreateGLBResource>();

		/// <summary>
		/// Adds a new type of node to the Registry.
		/// </summary>
		/// <typeparam name="T">The type of the node being registered.</typeparam>
		/// <returns>The Registry with the newly added node.</returns>
		public Registry Register<T>() where T : ACustomNode
			=> Register<T>(typeof(T).Name);

		/// <summary>
		/// Adds a new type of node to the Registry under a given name.
		/// </summary>
		/// <param name="name">Should correspond to the node Type defined in the Blueprint, of the node you are implementing.</param>
		/// <typeparam name="T">The type of the node being registered.</typeparam>
		/// <returns>The Registry with the newly added node.</returns>
		public Registry Register<T>(string name) where T : ACustomNode
		{
			fixed (char* nameUtf16 = name)
			{
				Calls.registry_register_node(
					NativePtr,
					(ushort*)nameUtf16,
					name.Length,
					(IntPtr)GCHandle.Alloc(typeof(T)),
					CustomNodeExecute,
					Memory.ReleaseGCHandle
				);
			}

			return this;
		}

		[MonoPInvokeCallback(typeof(Calls.registry_register_node_execute_cb_delegate))]
		private static void CustomNodeExecute(
			nint thisPtr,
			byte* nodeIdUtf8,
			uint scopeId,
			ArcExecutionContext* ctxPtr)
		{
			var handle = GCHandle.FromIntPtr(thisPtr);
			var nodeType = (Type)handle.Target;

			// load context
			var localDataPtr = new Dynamic(Calls.ctx_get_context_data(ctxPtr));
			if (!localDataPtr.TryDeref<ExecutionContext.ContextData>(out var contextData))
			{
				throw new Exception("Failed to read local data from ExecutionContext");
			}

			var context = new ExecutionContext(ctxPtr, contextData);

			var nodeId = Marshal.PtrToStringUTF8((IntPtr)nodeIdUtf8);
			var instance = (ACustomNode)Activator.CreateInstance(
				nodeType,
				new ACustomNode.Context(nodeId, scopeId, context)
			);
			UBFPlayerLoopSystem.ExecuteNode(
				instance,
				nodeId,
				scopeId,
				context
			);
		}
	}
}