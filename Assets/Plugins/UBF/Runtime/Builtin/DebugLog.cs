// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class DebugLog : ACustomNode
	{
		public static Action<string> OnLog;
		public DebugLog(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (TryRead<string>("Message", out var message))
			{
				Debug.Log(message);
				OnLog?.Invoke(message);
			}
			else
			{
				Debug.LogWarning("DebugLog: Message not found");
			}

			TriggerNext();
		}
	}
}