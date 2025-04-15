// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Runtime.InteropServices;
using AOT;

namespace Futureverse.UBF.Runtime.Native
{
	internal static class Memory
	{
		[MonoPInvokeCallback(typeof(FFI.Calls.registry_register_node__release_cb_delegate))]
		internal static void ReleaseGCHandle(nint thisPtr)
		{
			var handle = GCHandle.FromIntPtr(thisPtr);
			handle.Free();
		}
	}
}