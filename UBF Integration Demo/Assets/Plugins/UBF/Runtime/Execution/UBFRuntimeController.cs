// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Futureverse.UBF.Runtime.Execution
{
	/// <summary>
	/// MonoBehaviour wrapper for UBF Blueprint execution that can hide the object until the Blueprint is fully finished,
	/// and can clear all object on successive executions.
	/// </summary>
	public class UBFRuntimeController : MonoBehaviour
	{
		[SerializeField] private bool _disableWhileExecuting;
		[SerializeField] private bool _destroyPreviousOnExecute = true;
		[SerializeField] private UnityEvent<ExecutionResult> _onExecutionComplete;

		/// <summary>
		/// Calls UBFExecutor.ExecuteRoutine while optionally clearing GameObject spawned by the previous execution, and
		/// optionally hiding all child objects while the Blueprint is executing.
		/// </summary>
		/// <param name="artifactProvider">Provides a way for Remote resources to be loaded</param>
		/// <param name="rootInstanceId">The instance ID of the Blueprint that should be initially executed.</param>
		/// <param name="blueprintInstances">A list of Blueprint instances to preload before execution.</param>
		/// <param name="onComplete">Callback containing the result of the execution.</param>
		/// <returns>An IEnumerator to yield on.</returns>
		public IEnumerator Execute(
			string rootInstanceId,
			IArtifactProvider artifactProvider,
			List<IBlueprintInstanceData> blueprintInstances,
			Action<ExecutionResult> onComplete = null)
		{
			if (_destroyPreviousOnExecute)
			{
				DestroyChildObjects();
			}

			if (_disableWhileExecuting)
			{
				transform.gameObject.SetActive(false);
			}

			onComplete += ((x) => _onExecutionComplete.Invoke(x));
			yield return UBFExecutor.ExecuteRoutine(
				new ExecutionData(
					transform,
					onComplete,
					blueprintInstances,
					artifactProvider
				),
				rootInstanceId
			);

			if (_disableWhileExecuting)
			{
				transform.gameObject.SetActive(true);
			}
		}

		/// <summary>
		/// Clear all GameObjects that were created by UBF Blueprint executions
		/// </summary>
		public void DestroyChildObjects()
		{
			foreach (Transform child in transform)
			{
				Destroy(child.gameObject);
			}
		}
	}
}