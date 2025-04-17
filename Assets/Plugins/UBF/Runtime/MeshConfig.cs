using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshConfig", menuName = "UBF/Mesh Config")]
public class MeshConfig : ScriptableObject
{
    public GameObject RigPrefab;
    public Avatar avatar;
}

public class RuntimeMeshConfig
{
    public MeshConfig Config;
    public GameObject RuntimeObject;
}

public class RigUtilities : MonoBehaviour
{
    public static void RetargetRig(SkinnedMeshRenderer source, SkinnedMeshRenderer target)
    {
        // This function requires both rigs to have the same bone names
        // It is designed to facilitate retargeting where those bones are in a different order on another mesh
        // Retargeting to different bone names will require a different map
        Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
        foreach (var bone in source.bones)
        {
            boneMap[bone.name] = bone;
        }

        Transform[] boneArray = target.bones;
        for (var idx = 0; idx < boneArray.Length; ++idx)
        {
            var boneName = boneArray[idx].name;
            if (!boneMap.TryGetValue(boneName, out boneArray[idx]))
            {
                Debug.LogError("failed to get bone: " + boneName);
                Debug.Break();
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

        Transform[] boneArray = target.bones;
        for (var idx = 0; idx < boneArray.Length; ++idx)
        {
            var boneName = boneArray[idx].name;
            if (!boneMap.TryGetValue(boneName, out boneArray[idx]))
            {
                Debug.LogError("failed to get bone: " + boneName);
                Debug.Break();
            }
        }

        target.bones = boneArray; //take effect
    }

    public static void RetargetRig(Transform sourceRoot, SkinnedMeshRenderer target)
    {
        List<Transform> boneList = new List<Transform>();
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
}
