// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class CreateMeshConfig : ACustomNode
	{
		public CreateMeshConfig(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryReadResourceId("Resource", out var resourceId))
			{
				Debug.LogWarning("Spawn Mesh node could not find Resource from pin: Resource");
				TriggerNext();
				return;
			}

			WriteOutput("MeshConfig", Dynamic.Foreign(null));
			TriggerNext();
		}
	}
}