// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Linq;
using Futureverse.UBF.Runtime.Settings;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class CreateMeshConfig : ACustomExecNode
	{
		public CreateMeshConfig(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryReadResourceId("Resource", out var resourceId) || !resourceId.IsValid)
			{
				UbfLogger.LogError("[CreateMeshConfig] Could not find resource input \"Resource\"");
				return;
			}

			if (!TryRead("ConfigOverrideKey", out string configKey))
			{
				UbfLogger.LogError("[CreateMeshConfig] Could not find input \"ConfigOverrideKey\"");
				return;
			}

			var settings = UBFSettings.GetOrCreateSettings();
			if (settings == null)
			{
				UbfLogger.LogWarn("[CreateMeshConfig] Unable to read UBF settings");
				return;
			}
			var configEntry = settings
				.MeshConfigs?.FirstOrDefault(x => x.Key == configKey);

			RuntimeMeshConfig runtimeConfig = null;
			if (configEntry != null && configEntry.Config != null && configEntry.Config.RigPrefab != null)
			{
				var spawnedRig = Object.Instantiate(configEntry.Config.RigPrefab, NodeContext.ExecutionContext.Config.GetRootTransform);
				runtimeConfig = new RuntimeMeshConfig()
				{
					Config = configEntry.Config,
					RuntimeObject = spawnedRig,
				};
			}

			WriteOutput("MeshConfig", runtimeConfig);
		}
	}
}