// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class BindMeshes : ACustomExecNode
	{
		public BindMeshes(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryReadArray<MeshRendererSceneComponent>("Meshes", out var meshes))
			{
				UbfLogger.LogError("[BindMeshes] Could not find input \"Mesh\"");
				return;
			}

			if (!TryRead<RigSceneComponent>("Rig", out var rig))
			{
				UbfLogger.LogError("[BindMeshes] Could not find input \"Rig\"");
				return;
			}

			Bind(rig, meshes);
		}

		private void Bind(RigSceneComponent rig, List<MeshRendererSceneComponent> targetSkins)
		{
			var rigRenderers = rig.GetRenderers();
			if (rigRenderers.Length == 0 || rigRenderers.All(x => !x.skinned))
			{
				return;
			}

			// At least one item having .skinned means it has at least 1 renderer, and that renderer is definitely a SkinnedMeshRenderer
			var rootBone = (rigRenderers.First(x => x.skinned).TargetMeshRenderers[0] as SkinnedMeshRenderer).rootBone;
			var boneDictionary = new Dictionary<string, Transform>();
			var rootBoneChildren = rootBone.GetComponentsInChildren<Transform>();
			foreach (var child in rootBoneChildren)
			{
				boneDictionary[child.name] = child;
			}

			foreach (var targetSkin in targetSkins)
			{
				if (!targetSkin.skinned)
				{
					continue;
				}

				foreach (var tRender in targetSkin.TargetMeshRenderers)
				{
					var skin = tRender as SkinnedMeshRenderer;
					var newBones = new Transform[skin.bones.Length];
					for (var i = 0; i < skin.bones.Length; i++)
					{
						if (skin.bones[i] == null)
						{
							continue;
						}

						if (boneDictionary.TryGetValue(skin.bones[i].name, out var newBone))
						{
							newBones[i] = newBone;
						}
					}

					skin.bones = newBones;
				}
			}
		}
	}
}