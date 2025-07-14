// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Futureverse.UBF.Runtime.Utils;
using GLTFast;
using UnityEngine;

namespace Plugins.UBF.Runtime.Utils
{
	public static class RigUtils
	{
        // In these, source refers to the rig that the target rig will point to, aka the rig that will be animated
        // translation used like translating one word to another (e.g translate target bone name into matching bone name)

		public static void RetargetRig(SkinnedMeshRenderer source, SkinnedMeshRenderer target, Dictionary<string,string> translation = null)
        {
            var boneMap = new Dictionary<string, Transform>();
            foreach (var bone in source.bones)
            {
                boneMap[bone.name] = bone;
            }

            var boneArray = target.bones;
            for (var idx = 0; idx < boneArray.Length; ++idx)
            {
                var boneName = boneArray[idx].name;
                if (translation != null && translation.TryGetValue(boneName, out var translationValue))
                {
                    boneName = translationValue;
                }
                if (!boneMap.TryGetValue(boneName, out boneArray[idx]))
                {
                    UbfLogger.LogError($"Failed to get bone \"{boneName}\"");
                }
            }

            target.bones = boneArray; //take effect
            target.rootBone = source.rootBone;
        }

        public static void RetargetRig(IEnumerable<Transform> source, SkinnedMeshRenderer target, Dictionary<string,string> translation = null) 
        {
            Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
            foreach (var bone in source)
            {
                boneMap[bone.name] = bone;
            }

            // iterate over the bones to be retargeted
            // if a matching bone exists on the anim rig, then retarget to point at that
            // if translation map exists, convert the name of the bone on the target rig to the one used in the source rig
            var boneArray = target.bones; 
            for (var idx = 0; idx < boneArray.Length; ++idx)
            {
                var boneName = boneArray[idx].name;
                if (translation != null && translation.TryGetValue(boneName, out var translationValue))
                {
                    boneName = translationValue;
                }
                if (!boneMap.TryGetValue(boneName, out boneArray[idx]))
                {
                    UbfLogger.LogError($"Failed to get bone \"{boneName}\"");
                }
            }

            target.bones = boneArray; // take effect
        }

        public static void RetargetRig(Transform sourceRoot, SkinnedMeshRenderer target, Dictionary<string,string> translation = null)
        {
            var boneList = new List<Transform>();
            GetAllChildren(sourceRoot, boneList);
            RetargetRig(boneList, target, translation);
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