using UnityEngine;

public class RigVisualizer : MonoBehaviour
{
    [Tooltip("Radius of the spheres drawn at each bone.")]
    public float boneSphereRadius = 0.01f;

    [Tooltip("Color of the spheres at each bone.")]
    public Color boneColor = Color.green;

    [Tooltip("Color of the lines connecting parent and child bones.")]
    public Color linkColor = Color.yellow;

    private void OnDrawGizmos()
    {
        if (transform == null)
            return;

        DrawBoneRecursive(transform);
    }

    private void DrawBoneRecursive(Transform current)
    {
        Gizmos.color = boneColor;
        Gizmos.DrawSphere(current.position, boneSphereRadius);

        foreach (Transform child in current)
        {
            // Draw link to child
            Gizmos.color = linkColor;
            Gizmos.DrawLine(current.position, child.position);

            // Recursively draw the child's bones
            DrawBoneRecursive(child);
        }
    }
}