// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Text;
using AOT;
using Futureverse.UBF.Runtime.Native.FFI;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Utils
{
	internal class UbfLogger
	{
		public static void LogInfo(object message) 
		{
			var m = $"[UBF] {message}".Replace("] [", "][");
			Debug.Log(m);
		}
		
		public static void LogWarn(object message) 
		{
			var m = $"[UBF] {message}".Replace("] [", "][");
			Debug.LogWarning(m);
		}
		
		public static void LogError(object message)
		{
			var m = $"[UBF] {message}".Replace("] [", "][");
			Debug.LogError(m);
		}
		
		[MonoPInvokeCallback(typeof(Calls.graph_load_on_log_delegate))]
		public static unsafe void GraphLoadLogCallback(int logLevel, byte* message, int messageLength)
		{
			var m = $"[DLL] {Encoding.UTF8.GetString(message, messageLength)}";
			switch (logLevel)
			{
				case 1:
					LogWarn(m);
					break;
				case 2:
					LogError(m);
					break;
				default:
					LogInfo(m);
					break;
			}
		}
		
		[MonoPInvokeCallback(typeof(Calls.graph_execute_on_log_delegate))]
		public static unsafe void GraphExecuteLogCallback(int logLevel, byte* message, int messageLength, Native.FFI.Dynamic* context)
		{
			// Currently we don't do anything with the context, so just do exactly what the above callback is doing
			GraphLoadLogCallback(logLevel, message, messageLength);
		}
	}
}