// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Threading.Tasks;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Utils
{
	/// <summary>
	/// Custom yield instruction that yields until a System.Task is completed.
	/// </summary>
	public class WaitForTask : CustomYieldInstruction
	{
		private readonly Task _task;

		public WaitForTask(Task task)
		{
			_task = task;
		}

		public override bool keepWaiting => !_task.IsCompleted;
	}
}