// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using UnityEngine;

namespace Futureverse.UBF.Runtime.Utils
{
	/// <summary>
	/// MonoBehaviour singleton that can be used to run Coroutines from non-MonoBehaviour classes.
	/// </summary>
	public class CoroutineHost : MonoBehaviour
	{
		private static CoroutineHost s_instance;

		/// <summary>
		/// This will create the MonoBehaviour in the scene if it does not already exist. Otherwise, assigns the first
		/// CoroutineHost object it finds in the scene.
		/// </summary>
		public static CoroutineHost Instance
		{
			get
			{
				if (s_instance != null)
				{
					return s_instance;
				}

				var existingController = FindFirstObjectByType<CoroutineHost>();
				if (existingController != null)
				{
					s_instance = existingController;
				}
				else
				{
					var newGo = new GameObject("Coroutine Host");
					s_instance = newGo.AddComponent<CoroutineHost>();
				}

				return s_instance;
			}
		}
	}
}