// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class ExecuteBlueprint : ACustomNode
	{
		public ExecuteBlueprint(Context context) : base(context) { }

		protected override IEnumerator ExecuteAsync()
		{
			if (!TryReadResourceId("Blueprint", out var resourceId) || !resourceId.IsValid)
			{
				Debug.Log("[ExecuteBlueprint] No blueprint resource provided");
				TriggerNext();
				yield break;
			}

			Blueprint blueprint = null;
			yield return NodeContext.ExecutionContext.Config.GetBlueprintInstance(resourceId, g => blueprint = g);
			if (blueprint == null)
			{
				Debug.LogError($"[ExecuteBlueprint] Unable to get or create instance from ID {resourceId.Value}");
				TriggerNext();
				yield break;
			}

			var declaredInputs = NodeContext.ExecutionContext.GetDeclaredNodeInputs(NodeContext.NodeId);
			foreach (var declaredInput in declaredInputs)
			{
				if (declaredInput == "Blueprint")
				{
					continue;
				}

				if (TryRead(declaredInput, out var dynamic))
				{
					blueprint.RegisterVariable(declaredInput, dynamic.AsObject());
				}
			}

			var execTask = new BlueprintExecutionTask(blueprint, NodeContext.ExecutionContext.Config);
			yield return execTask;

			// forward graph outputs to node outputs
			foreach (var output in blueprint.Outputs)
			{
				if (execTask.ExecutionContext.TryReadOutput(output.Id, out var value))
				{
					Debug.Log($"Forwarding Output; binding-id={output.Id}; value={value}");
					WriteOutput(output.Id, value);
				}
			}

			//WriteOutput("Success", Dynamic.Bool(true));

			TriggerNext();
		}
	}
}