using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigMirror : MonoBehaviour
{
    public Transform sourceRoot;   // Root of animated rig
    public Transform targetRoot;   // Root of target rig

    private Dictionary<string, Transform> sourceBones = new Dictionary<string, Transform>();
    private Dictionary<string, Transform> targetBones = new Dictionary<string, Transform>();

    private bool assigned = false;

    public void Assign(Transform source)
    {
        Assign(source, targetRoot);
    }
    
    public void Assign(Transform source, Transform target)
    {
        sourceRoot = source;
        targetRoot = target;
        // Populate bone dictionaries
        foreach (var bone in sourceRoot.GetComponentsInChildren<Transform>())
            sourceBones[bone.name] = bone;
        
        foreach (var bone in targetRoot.GetComponentsInChildren<Transform>())
        {
            if (sourceBones.ContainsKey(bone.name))
            {
                targetBones[bone.name] = bone;
            }
            else
            {
                //bone.gameObject.SetActive(false);
            }
        }
        assigned = true;
    }

    void LateUpdate()
    {
        if (!assigned) return;
        // Mirror transforms from source to target
        foreach (var kv in targetBones)
        {
            if (sourceBones.TryGetValue(kv.Key, out var sourceBone))
            {
                kv.Value.position = sourceBone.position;
                kv.Value.rotation = sourceBone.rotation;
                kv.Value.localScale    = sourceBone.localScale;
            }
        }
    }
}
