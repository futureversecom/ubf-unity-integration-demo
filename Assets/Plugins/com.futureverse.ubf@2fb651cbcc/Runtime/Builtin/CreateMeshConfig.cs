// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using System.Linq;
using Futureverse.UBF.Runtime.Settings;
using Futureverse.UBF.Runtime.Utils;
using GLTFast;
using Plugins.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class CreateMeshConfig : ACustomExecNode
	{
		public CreateMeshConfig(Context context) : base(context) { }

		protected override IEnumerator ExecuteAsync()
		{
			if (!TryReadResourceId("Resource", out var resourceId) || !resourceId.IsValid)
			{
				UbfLogger.LogError("[CreateMeshConfig] Could not find resource input \"Resource\"");
				yield break;
			}

			if (!TryRead("ConfigOverrideKey", out string configKey))
			{
				UbfLogger.LogError("[CreateMeshConfig] Could not find input \"ConfigOverrideKey\"");
				yield break;
			}

			var settings = UBFSettings.GetOrCreateSettings();
			if (settings == null)
			{
				UbfLogger.LogWarn("[CreateMeshConfig] Unable to read UBF settings");
				yield break;
			}
			
			var configEntry = settings
				.MeshConfigs?.FirstOrDefault(x => x.Key == configKey);

			if (configEntry == null && settings.MeshConfigs.Any(x => x.Key == "Default"))
			{
				configEntry = settings.MeshConfigs?.FirstOrDefault(x => x.Key == "Default");
			}
			
			
			RuntimeMeshConfig runtimeConfig = new RuntimeMeshConfig
			{
				Config = ScriptableObject.CreateInstance<MeshConfig>()
			};
			

			/*
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
			*/
			yield return CreateAvatar(resourceId, runtimeConfig, configEntry);
			
			WriteOutput("MeshConfig", runtimeConfig);
		}

		private IEnumerator CreateAvatar(ResourceId resourceId, RuntimeMeshConfig config, UBFSettings.MeshConfigEntry configEntry)
		{
			GltfImport gltfResource = null;
			var routine = CoroutineHost.Instance.StartCoroutine(
				NodeContext.ExecutionContext.Config.GetMeshInstance(
					resourceId,
					(resource, _) =>
					{
						gltfResource = resource;
					}
				)
			);
			if (routine != null)
			{
				yield return routine;
			}

			if (gltfResource == null)
			{
				UbfLogger.LogError($"[SpawnModel] Could not load GLB resource with Id \"{resourceId.Value}\"");
				yield break;
			}
			
			var instantiator = new GameObjectInstantiator(gltfResource, new GameObject("Temp_GLTF_Config").transform);

			var instantiateRoutine = CoroutineHost.Instance.StartCoroutine(
				new WaitForTask(gltfResource.InstantiateMainSceneAsync(instantiator))
			);
			if (instantiateRoutine != null)
			{
				yield return instantiateRoutine;
			}

			config.Config.Avatar =
				RigUtils.CreateAvatar(instantiator.SceneTransform, configEntry.Config.avatarMap);
			
			Object.Destroy(instantiator.SceneTransform.gameObject);
		}
	}
}