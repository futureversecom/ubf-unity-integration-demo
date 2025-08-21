// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class ExecuteBlueprint : ACustomExecNode
	{
		public ExecuteBlueprint(Context context) : base(context) { }

		protected override IEnumerator ExecuteAsync()
		{
			if (!TryReadResourceId("Blueprint", out var resourceId) || !resourceId.IsValid)
			{
				yield break;
			}

			Blueprint blueprint = null;
			var routine = CoroutineHost.Instance.StartCoroutine(NodeContext.ExecutionContext.Config.GetBlueprintInstance(
				resourceId,
				(bp, _) => blueprint = bp
			));
			if (routine != null)
			{
				yield return routine;
			}
			if (blueprint == null)
			{
				UbfLogger.LogError($"[ExecuteBlueprint] Unable to get or create Blueprint Instance from ID {resourceId.Value}");
				yield break;
			}

			var declaredInputs = NodeContext.ExecutionContext.GetDeclaredNodeInputs(NodeContext.NodeId);
			foreach (var declaredInput in declaredInputs)
			{
				if (declaredInput == "Blueprint")
				{
					continue;
				}

				if (TryRead("In." + declaredInput, out var dynamic))
				{
					blueprint.RegisterVariable(declaredInput, dynamic);
				}
			}

			var execTask = new BlueprintExecutionTask(blueprint, NodeContext.ExecutionContext.Config);
			yield return execTask;

			// forward graph outputs to node outputs
			foreach (var output in blueprint.Outputs)
			{
				if (execTask.ExecutionContext.TryReadOutput("Out." + output.Id, out var value))
				{
					WriteOutput(output.Id, value);
				}
			}
		}
	}
}