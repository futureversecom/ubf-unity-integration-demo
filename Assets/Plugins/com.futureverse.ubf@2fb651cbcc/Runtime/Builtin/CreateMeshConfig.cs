// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Linq;
using Futureverse.UBF.Runtime.Settings;
using Futureverse.UBF.Runtime.Utils;
using GLTFast;
using Plugins.UBF.Runtime.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

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

			if (!TryRead("ConfigKey", out string configKey))
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

			if (configEntry == null)
			{
				UbfLogger.LogError("[CreateMeshConfig] Could not find config entry \"Default\"");
				WriteOutput("MeshConfig", null);
				yield break;
			}
			
			RuntimeMeshConfig runtimeConfig = new RuntimeMeshConfig
			{
				Config = ScriptableObject.CreateInstance<MeshConfig>()
			};
			
			// Instantiate the resource provided to the CreateMeshConfig node. 
			// Use it to create an avatar, and serve as the core animation reference
			// Both game rig and any models spawned with this config will retarget to follow the animation rig
			yield return SetupAnimationObject(NodeContext.ExecutionContext.Config.GetRootTransform, resourceId, runtimeConfig, configEntry);
			
			// If a game rig is present, spawn and point at anim rig
			if (configEntry.Config.RigPrefab != null)
			{
				var spawnedRig = Object.Instantiate(configEntry.Config.RigPrefab, NodeContext.ExecutionContext.Config.GetRootTransform);
				runtimeConfig.GameLogicObject = spawnedRig;
				var mirror = spawnedRig.GetComponent<RigMirror>();
				if (mirror != null)
				{
					mirror.Assign(runtimeConfig.AnimationObject.transform);
				}
			}
			WriteOutput("MeshConfig", runtimeConfig);
		}

		private IEnumerator SetupAnimationObject(Transform rootTransform, ResourceId resourceId, RuntimeMeshConfig config, UBFSettings.MeshConfigEntry configEntry)
		{
			// Instantiate the resource
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
			
			// Once spawned, move it to the top of the sibling tree so that it is the object that gets targeted by the parent animator component
			var root = new GameObject("Avatar Root").transform;
			root.SetParent(rootTransform);
			root.SetSiblingIndex(0);
			var instantiator = new GameObjectInstantiator(gltfResource, root);

			var instantiateRoutine = CoroutineHost.Instance.StartCoroutine(
				new WaitForTask(gltfResource.InstantiateMainSceneAsync(instantiator))
			);
			if (instantiateRoutine != null)
			{
				yield return instantiateRoutine;
			}

			// Use the 'avatar' map to create a T-Pose avatar. Only compatible with ARP models
			config.Config.SetAvatar(RigUtils.CreateAvatar(instantiator.SceneTransform, configEntry.Config.avatarMap));

			config.AnimationObject = instantiator.SceneTransform.gameObject;

			// Remove any materials from the animation object renderers
			// Need to keep the components present / enabled so they can process skeletol animation
			foreach (var renderer in config.AnimationObject.GetComponentsInChildren<Renderer>())
			{
				renderer.materials = Array.Empty<Material>();
			}
			// Set the avatar to the animator
			var animator = instantiator.SceneTransform.GetComponentInParent<Animator>(includeInactive: true); // TODO make this a variable in the graph execution data?
			animator.avatar = config.Config.Avatar;
		}
	}
}