// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime;
using Futureverse.UBF.Runtime.Utils;
using GLTFast;
using UnityEngine;

namespace Plugins.UBF.Runtime.Utils
{
	public static class RigUtils
	{
		public static void RetargetRig(SkinnedMeshRenderer source, SkinnedMeshRenderer target)
        {
            // This function requires both rigs to have the same bone names
            // It is designed to facilitate retargeting where those bones are in a different order on another mesh
            // Retargeting to different bone names will require a different map
            var boneMap = new Dictionary<string, Transform>();
            foreach (var bone in source.bones)
            {
                boneMap[bone.name] = bone;
            }

            var boneArray = target.bones;
            for (var idx = 0; idx < boneArray.Length; ++idx)
            {
                var boneName = boneArray[idx].name;
                if (!boneMap.TryGetValue(boneName, out boneArray[idx]))
                {
                    UbfLogger.LogError($"Failed to get bone \"{boneName}\"");
                }
            }

            target.bones = boneArray; //take effect
            target.rootBone = source.rootBone;
        }

        public static void RetargetRig(IEnumerable<Transform> source, SkinnedMeshRenderer target)
        {
            Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
            foreach (var bone in source)
            {
                boneMap[bone.name] = bone;
            }

            var boneArray = target.bones;
            
            for (var idx = 0; idx < boneArray.Length; ++idx)
            {
                var boneName = boneArray[idx].name;
                if (!boneMap.TryGetValue(boneName, out boneArray[idx]))
                {
                    UbfLogger.LogError($"Failed to get bone \"{boneName}\"");
                }
            }

            target.bones = boneArray; //take effect
        }

        public static void RetargetRig(Transform sourceRoot, SkinnedMeshRenderer target)
        {
            var boneList = new List<Transform>();
            GetAllChildren(sourceRoot, boneList);
            RetargetRig(boneList, target);
            target.rootBone = sourceRoot;
        }

        public static void GetAllChildren(Transform parent, List<Transform> children)
        {
            foreach (Transform child in parent)
            {
                children.Add(child);
                GetAllChildren(child, children);
            }
        }

        public static bool IsEqual(this GltfImport a, GltfImport b)
        {
            var aNodes = a.GetSourceRoot()
                .Nodes;
            var bNodes = b.GetSourceRoot()
                .Nodes;
            
            if (aNodes.Count != bNodes.Count)
            {
                return false;
            }

            for (var i = 0; i < aNodes.Count; ++i)
            {
                var aNode = aNodes[i];
                var bNode = bNodes[i];

                if (aNode.name != bNode.name)
                {
                    return false;
                }

                if (aNode.mesh != bNode.mesh)
                {
                    return false;
                }

                if (aNode.skin != bNode.skin)
                {
                    return false;
                }
            }

            return true;
        }
        
        public static Avatar CreateAvatar(Transform boneSource, List<MeshConfig.ConfigMapItem> map)
        {
            var desc = new HumanDescription();
            var human = new HumanBone[map.Count];
            var skeleton = new SkeletonBone[map.Count];

            for (int i = 0; i < human.Length; i++)
            {
                var bone = new HumanBone();
                bone.humanName = map[i].sourceBoneName;
                bone.boneName = map[i].targetBoneName;
                bone.limit = new HumanLimit() { useDefaultValues = true };
                human[i] = bone;
            
                var t = boneSource.FindRecursive(map[i].targetBoneName);
                if (t == null)
                {
                    Debug.LogError($"Cannot find avatar bone for {map[i].targetBoneName}");
                }
                skeleton[i] = new SkeletonBone()
                {
                    name = map[i].sourceBoneName,
                    position = t.position,
                    rotation = t.rotation,
                    scale = t.localScale
                };

            }

            desc.human = human;
            desc.skeleton = CreateSkeleton(boneSource.gameObject);
            desc.upperArmTwist = 0.5f;
            desc.lowerArmTwist = 0.5f;
            desc.upperLegTwist = 0.5f;
            desc.lowerLegTwist = 0.5f;
            desc.armStretch = 0.05f;
            desc.legStretch = 0.05f;
            desc.feetSpacing = 0f;
            desc.hasTranslationDoF = false;
            var rtAvatar = AvatarBuilder.BuildHumanAvatar(boneSource.gameObject, desc);

            return rtAvatar;
        }
        
        private static SkeletonBone[] CreateSkeleton(GameObject avatarRoot)
        {
            List<SkeletonBone> skeleton = new List<SkeletonBone>();

            Transform[] avatarTransforms = avatarRoot.GetComponentsInChildren<Transform>();
            foreach (Transform avatarTransform in avatarTransforms)
            {
                SkeletonBone bone = new SkeletonBone()
                {
                    name = avatarTransform.name,
                    position = avatarTransform.localPosition,
                    rotation = avatarTransform.localRotation,
                    scale = avatarTransform.localScale
                };

                skeleton.Add(bone);
            }
            string[] names = skeleton.Select(x => x.name).ToArray();
            Debug.Log(string.Join('\n', names));
            return skeleton.ToArray();
        }
        
        public static Transform FindRecursive(this Transform transform, string name) {
            if(transform == null) return null;
            int count = transform.childCount;
            for(int i = 0; i < count; i++) {
                Transform child = transform.GetChild(i);
                if(child.name == name) return child;
                Transform subChild = FindRecursive(child, name);
                if(subChild != null) return subChild;
            }
            return null;
        }
	}
}