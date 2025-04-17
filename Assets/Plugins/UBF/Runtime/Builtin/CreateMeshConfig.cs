// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Linq;
using Futureverse.UBF.Runtime.Settings;
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
				//Debug.LogWarning("Spawn Mesh node could not find Resource from pin: Resource");
				//TriggerNext();
				//return;
			}

			if (!TryRead("ConfigOverrideKey", out string configKey))
			{
				Debug.LogWarning("Could not read ConfigOverrideKey from pin: ConfigOverrideKey");
				TriggerNext();
				return;
			}

			var configEntry = UBFSettings.GetOrCreateSettings()
				.MeshConfigs.FirstOrDefault(x => x.Key == configKey);

			RuntimeMeshConfig runtimeConfig = null;
			if (configEntry != null)
			{
				Debug.Log("Found ConfigOverrideKey: " + configEntry.Key);
				Debug.Log("Spawning RigPrefab: " + configEntry.Config.RigPrefab.name);
				var spawnedRig = Object.Instantiate(configEntry.Config.RigPrefab, NodeContext.ExecutionContext.Config.GetRootTransform);
				runtimeConfig = new RuntimeMeshConfig()
				{
					Config = configEntry.Config,
					RuntimeObject = spawnedRig
				};
			}

			var foreignOut = Dynamic.Foreign(runtimeConfig);
			WriteOutput("MeshConfig", foreignOut);
			TriggerNext();
		}
	}
}