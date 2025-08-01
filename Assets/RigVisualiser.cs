using System;
using System.Collections;
using System.Collections.Generic;
using Plugins.UBF.Runtime.Utils;
using UnityEngine;

public class RigVisualiser : MonoBehaviour
{
    public Transform[] children;
    public Color sphereColor;
    public float radius = 0.05f;
    
    private void OnDrawGizmosSelected()
    {
        if (children == null || children.Length == 0)
        {
            return;
        }

        Gizmos.color = sphereColor;
        foreach (var child in children)
        {
            Gizmos.DrawSphere(child.position, radius);
        }
    }
    
    [ContextMenu("Populate")]
    public void PopulateChildren()
    {
        children = GetComponentsInChildren<Transform>();
    }
    
}
