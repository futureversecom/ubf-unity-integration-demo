using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigVisualiser : MonoBehaviour
{
    private List<Transform> points = new();
    private List<(Transform, Transform)> edges = new();

    public float radius = 0.02f;
    public bool show = false;
    
    private void Start()
    {
        GetPointsAndEdges();

        show = true;
    }

    [ContextMenu("Refresh")]
    private void GetPointsAndEdges()
    {
        points.Clear();
        edges.Clear();
        var open = new List<Transform>
        {
            this.transform,
        };

        while (open.Count > 0)
        {
            var current = open[0];
            open.Remove(current);
            
            points.Add(current);
            foreach (Transform child in current)
            {
                open.Add(child);
                edges.Add((current,child));
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!show)
            return;

        if (points.Count == 0)
        {
            GetPointsAndEdges();
        }
        Gizmos.color = Color.blue;

        try
        {
            foreach (var point in points)
            {
                Gizmos.DrawWireSphere(point.position, radius);
            }

            Gizmos.color = Color.cyan;
            foreach (var edge in edges)
            {
                Gizmos.DrawLine(edge.Item1.position, edge.Item2.position);
            }
        }
        catch
        {
            GetPointsAndEdges();
        }
        
    }
}