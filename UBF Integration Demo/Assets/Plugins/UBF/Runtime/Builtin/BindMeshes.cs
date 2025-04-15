// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class BindMeshes : ACustomNode
	{
		public BindMeshes(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryReadArray<Renderer>("Mesh", out var meshes))
			{
				Debug.LogError("[BindMesh] No input provided to pin \"Mesh\"");
				TriggerNext();
				return;
			}

			if (!TryRead<Renderer>("Skeleton", out var skeleton))
			{
				Debug.LogError("[BindMesh] No input provided to pin \"Skeleton\"");
				TriggerNext();
				return;
			}

			Bind(skeleton, meshes);
			TriggerNext();
		}

		private void Bind(Renderer skeleton, List<Renderer> targetSkins)
		{
			if (skeleton is not SkinnedMeshRenderer skinnedSkeleton)
			{
				return;
			}

			var rootBone = skinnedSkeleton.rootBone;
			var boneDictionary = new Dictionary<string, Transform>();
			var rootBoneChildren = rootBone.GetComponentsInChildren<Transform>();
			foreach (var child in rootBoneChildren)
			{
				boneDictionary[child.name] = child;
			}

			foreach (var targetSkin in targetSkins)
			{
				if (targetSkin is not SkinnedMeshRenderer skin)
				{
					continue;
				}

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
				skin.rootBone = rootBone;

			}
		}
	}
}