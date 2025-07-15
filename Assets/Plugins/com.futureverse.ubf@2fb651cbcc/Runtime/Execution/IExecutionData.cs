// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Execution
{
	/// <summary>
	/// Contains the data that must be passed into a UBF Execution.
	/// </summary>
	public interface IExecutionData
	{
		/// <summary>
		/// Callback containing the result of the Execution.
		/// </summary>
		Action<ExecutionResult> OnComplete { get; set; }
		/// <summary>
		/// Creates an instance of an IExecutionConfig that the Execution requires to run.
		/// </summary>
		/// <param name="callback">Callback containing the resulting IExecutionConfig</param>
		/// <returns>An IEnumerator to yield on.</returns>
		IEnumerator CreateExecutionConfig(Action<IExecutionConfig> callback);
	}

	public class ExecutionData : IExecutionData
	{
		public Action<ExecutionResult> OnComplete { get; set; }
		
		private readonly Transform _root;
		private readonly List<IBlueprintInstanceData> _blueprints;
		
		/// <param name="root">All objects spawned by a UBF execution with this config will be parented to this transform.</param>
		/// <param name="onComplete">Callback containing the Execution Result.</param>
		/// <param name="blueprints">List of data for Blueprints that should be preloaded.</param>
		public ExecutionData(
			Transform root,
			Action<ExecutionResult> onComplete,
			List<IBlueprintInstanceData> blueprints
			)
		{
			_root = root;
			OnComplete = onComplete;
			_blueprints = blueprints;
		}

		public IEnumerator CreateExecutionConfig(Action<IExecutionConfig> callback)
		{
			Dictionary<string, Blueprint> loadedGraphs = new();

			foreach (var blueprint in _blueprints)
			{
				var resourceId = ResourceId.UnsafeFromString(blueprint.ResourceId);
				yield return ArtifactProvider.Instance.GetBlueprintResource(
					resourceId,
					blueprint.InstanceId,
					(graph, _) =>
					{
						if (graph == null)
						{
							return;
						}
						
						foreach (var input in blueprint.Inputs)
						{
							graph.RegisterVariable(input.Key, input.Value);
						}

						loadedGraphs.Add(graph.InstanceId, graph);
					}
				);
			}

			callback?.Invoke(new ExecutionConfig(_root, loadedGraphs));
		}
	}
}