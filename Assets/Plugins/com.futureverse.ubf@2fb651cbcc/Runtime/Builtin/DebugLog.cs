// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class DebugLog : ACustomExecNode
	{
		public static Action<string> OnLog;
		public DebugLog(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (TryRead<string>("Message", out var message))
			{
				UbfLogger.LogInfo(message);
				OnLog?.Invoke(message);
			}
			else
			{
				UbfLogger.LogError("[DebugLog] Could not find input \"Message\"");
			}
		}
	}
}