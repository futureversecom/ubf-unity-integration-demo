using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Futureverse.UBF.Runtime
{
    public interface ISceneObject
    {
        string GetName();
    }
    
    public class SceneNode : ISceneObject
    {
        public string Name;
        public GameObject TargetSceneObject;
        public readonly List<SceneComponent> Components = new();
        public readonly List<SceneNode> Children = new();
        public SceneNode Parent;

        public void AddChild(SceneNode child, bool removeFromParent = true, bool reparentGameObjects = false)
        {
            Children.Add(child);
            if (removeFromParent && child.Parent != null)
            {
                child.Parent.Children.Remove(child);
            }

            if (Parent != null && Parent == child)
            {
                Parent = Parent.Parent ?? null;
            }

            if (reparentGameObjects)
            {
                child.TargetSceneObject.transform.SetParent(TargetSceneObject.transform);
            }
            
            child.Parent = this;
        }

        public void AddComponent(SceneComponent component, bool removeFromExisting = true)
        {
            Components.Add(component);
            if (component.Node != null && removeFromExisting)
            {
                component.Node.Components.Remove(component);
            }

            component.Node = this;
        }

        public void AddComponents(IEnumerable<SceneComponent> components, bool removeFromExisting = true)
        {
            foreach (var component in components)
            {
                AddComponent(component, removeFromExisting);
            }
        }

        #region Get Components

        public T GetComponent<T>() where T : SceneComponent
        {
            return Components.FirstOrDefault(x => x is T) as T;
        }

        public IEnumerable<T> GetComponents<T>() where T : SceneComponent
        {
            return Components.Where(x => x is T).Cast<T>();
        }
        
        public void GetComponent<T>(out T component) where T : SceneComponent
        {
            component = GetComponent<T>();
        }

        public void GetComponents<T>(out IEnumerable<T> components) where T : SceneComponent
        {
            components = GetComponents<T>();
        }

        public void GetComponent(Type T, out SceneComponent component)
        {
            component = Components.FirstOrDefault(x => x.GetType() == T);
        }

        public void GetComponents(Type T, out IEnumerable<SceneComponent> components)
        {
            components = Components.Where(x => x.GetType() == T);
        }

        #endregion

        
        
        public void PrintDebug()
        {
            DebugSceneTree(this);
        }
        
        public static SceneNode BuildSceneTree(Transform rootTransform, out List<SceneNode> allNodes)
        {
            allNodes = new List<SceneNode>();
            return BuildRecursive(rootTransform, allNodes);
        }

        private static SceneNode BuildRecursive(Transform t, List<SceneNode> all)
        {
            if (t == null) return null;

            var node = new SceneNode
            {
                TargetSceneObject = t.gameObject
            };
            all.Add(node);

            foreach (Transform child in t)
            {
                var childNode = BuildRecursive(child, all);
                if (childNode != null)
                    node.Children.Add(childNode);
            }

            return node;
        }
        
        public static void DebugSceneTree(SceneNode root)
        {
            if (root == null)
            {
                Debug.Log("(null root)");
                return;
            }

            StringBuilder sb = new StringBuilder();
            AppendNode(sb, root, 0);
            Debug.Log(sb.ToString());
        }

        private static void AppendNode(StringBuilder sb, SceneNode node, int depth)
        {
            string indent = new string(' ', depth * 2);
            string nodeName = node.TargetSceneObject != null ? node.TargetSceneObject.name : "(null)";
            sb.AppendLine($"{indent}{nodeName}");
            sb.AppendLine($"{indent}  Components ({node.Components.Count}): ");
            foreach (var comp in node.Components)
            {
                sb.AppendLine($"{indent}  - <{comp.GetType().Name}>: {comp.GetDebugString()}");
            }

            foreach (var child in node.Children)
            {
                AppendNode(sb, child, depth + 1);
            }
        }

        public string GetName()
        {
            return Name;
        }
    }

    public abstract class SceneComponent : ISceneObject
    {
        public SceneNode Node;

        public virtual string GetDebugString()
        {
            return String.Empty;
        }

        public virtual string GetName()
        {
            return "";
        }
    }

    public class MeshRendererSceneComponent : SceneComponent
    {
        public List<Renderer> TargetMeshRenderers;
        public bool skinned;

        public override string GetDebugString()
        {
            return $"[{TargetMeshRenderers.Count}]" + (TargetMeshRenderers.Count > 0 ? TargetMeshRenderers[0].name : "-");
        }

        public override string GetName()
        {
            return TargetMeshRenderers.Count > 0 ? TargetMeshRenderers[0].name : "";
        }
    }

    public class RigSceneComponent : SceneComponent
    {
        public SceneNode Root;
        public List<SceneNode> Bones = new();
        public override string GetDebugString()
        {
            return $"[{Bones.Count}]";
        }

        public MeshRendererSceneComponent[] GetRenderers()
        {
            return Node.Components.Where(x => x is MeshRendererSceneComponent).Cast<MeshRendererSceneComponent>().ToArray();
        }

        public static RigSceneComponent CreateFromSMR(SkinnedMeshRenderer smr)
        {
            var rigRoot = smr.rootBone;
            var rigRootNode = SceneNode.BuildSceneTree(rigRoot, out var boneNodes);
            var rig = new RigSceneComponent
            {
                Bones = boneNodes,
                Root = rigRootNode
            };
            return rig;
        }

        public override string GetName()
        {
            return Root?.TargetSceneObject?.name;
        }
    }

}
