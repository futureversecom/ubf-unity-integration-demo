// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using Futureverse.UBF.Runtime.Utils;

namespace Futureverse.UBF.Runtime.Execution
{
	public static class UBFExecutor
	{
		/// <summary>
		/// Sets up all necessary information for the execution using the data from the IExecutionData, runs
		/// the root Blueprint, and gives back the result via a callback.
		/// </summary>
		/// <param name="executionData">The data required for the Blueprints to run.</param>
		/// <param name="rootInstanceId">The Instance ID of the root Blueprint that should run first.</param>
		/// <returns>IEnumerator to yield on.</returns>
		public static IEnumerator ExecuteRoutine(IExecutionData executionData, string rootInstanceId)
		{
			IExecutionConfig executionConfig = null;
			Blueprint rootBlueprint = null;

			yield return executionData.CreateExecutionConfig(c => executionConfig = c);
			yield return executionConfig.GetBlueprintInstance(
				new ResourceId
				{
					Value = rootInstanceId,
				},
				(blueprint, _) => rootBlueprint = blueprint
			);

			if (rootBlueprint == null)
			{
				UbfLogger.LogError($"Cannot execute UBF. No root blueprint found with Instance ID {rootInstanceId}.");
				yield break;
			}

			var task = new BlueprintExecutionTask(rootBlueprint, executionConfig);
			yield return task;

			var result = new ExecutionResult(true, task.ExecutionContext);
			executionData.OnComplete?.Invoke(result);
		}
	}
}