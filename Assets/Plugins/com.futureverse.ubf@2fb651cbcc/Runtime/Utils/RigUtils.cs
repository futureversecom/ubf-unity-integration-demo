// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
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
	}
}