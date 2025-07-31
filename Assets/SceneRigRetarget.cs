using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime;
using Plugins.UBF.Runtime.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CustomEditor(typeof(SceneRigRetarget))]
public class SceneRigRetargetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (!Application.isPlaying) return;
        
        if (GUILayout.Button("Retarget - renderer"))
        {
            (target as SceneRigRetarget).Retarget_Renderer();
        }
        
        if (GUILayout.Button("Retarget - root"))
        {
            (target as SceneRigRetarget).Retarget_Root();
        }

        if (GUILayout.Button("Create Avatar"))
        {
            (target as SceneRigRetarget).CreateAvatar();
        }

        if (GUILayout.Button("Set Avatar"))
        {
            (target as SceneRigRetarget).SetAvatar();
        }
    }
}

public class SceneRigRetarget : MonoBehaviour
{
    public SkinnedMeshRenderer tPoseRenderer;
    public SkinnedMeshRenderer targetRenderer;
    public Transform tPoseSceneRoot;
    
    public Avatar prebuiltAvatar;
    public List<MeshConfig.ConfigMapItem> avatarOutput = new();
    public Avatar runtimeAvatar;
    public Animator runtimeAnimator;
    public Transform avatarBoneSource;
    
    private void OnValidate()
    {
        if (prebuiltAvatar != null && avatarOutput != null && avatarOutput.Count == 0)
        {
            avatarOutput = new();
            foreach (var bone in prebuiltAvatar.humanDescription.human)
            {
                avatarOutput.Add(new MeshConfig.ConfigMapItem(){sourceBoneName = bone.humanName, targetBoneName = bone.boneName});
            }
        }
    }

    public void Retarget_Renderer()
    {
        RigUtils.RetargetRig(tPoseRenderer, targetRenderer);
    }

    public void Retarget_Root()
    {
        RigUtils.RetargetRig(tPoseSceneRoot, targetRenderer);
    }

    [ContextMenu("Create avatar")]
    public void CreateAvatar()
    {
        runtimeAvatar = CreateAvatar(avatarBoneSource, avatarOutput);
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
        //desc.skeleton = skeleton;
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
        //runtimeAvatar.name = prebuiltAvatar.name;
        //var prebuiltDebug = GetAvatarDebug(prebuiltAvatar);
        //var runtimeDebug = GetAvatarDebug(rtAvatar);
        
        //Debug.Log($"PB Avatar: \n{prebuiltDebug}");
        //Debug.Log($"RT Avatar: \n{runtimeDebug}");
        return rtAvatar;
    }
    
    [ContextMenu("Set avatar")]
    public void SetAvatar()
    {
        runtimeAnimator.avatar = runtimeAvatar;
    }

    public string GetAvatarDebug(Avatar avatar)
    {
        string s = $"Name: {avatar.name}\n";
        s += "Human Bones: \n";
        foreach (var bone in avatar.humanDescription.human.OrderBy(x => x.humanName))
        {
            s += $"{bone.humanName} : {bone.boneName} ---\n";
            s += $"\tLimits: {bone.limit.center} : {bone.limit.min} / {bone.limit.max} : {bone.limit.axisLength} : {bone.limit.useDefaultValues}\n";
        }

        s += "Skeleton Bones: \n";
        foreach (var bone in avatar.humanDescription.skeleton.OrderBy(x => x.name))
        {
            s += $"{bone.name}\n\tPos: {bone.position}\n\tRot: {bone.rotation}\n\tScale: {bone.scale}\n";
        }

        s += $"UATwist: {avatar.humanDescription.upperArmTwist}\n";
        s += $"LATwist: {avatar.humanDescription.lowerArmTwist}\n";
        s += $"ULTwist: {avatar.humanDescription.upperLegTwist}\n";
        s += $"LLTwist: {avatar.humanDescription.lowerLegTwist}\n";
        s += $"ArmStretch: {avatar.humanDescription.armStretch}\n";
        s += $"FeetSpacing: {avatar.humanDescription.feetSpacing}\n";
        s += $"DoF: {avatar.humanDescription.hasTranslationDoF}\n";
        s += $"Human: {avatar.isHuman}\n";
        s += $"Valid: {avatar.isValid}\n";
        
        return s;
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
}

public static class RetargetExtensions
{
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
