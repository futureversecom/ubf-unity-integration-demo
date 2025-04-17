using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Execution;
using UnityEngine;

public class RigApplicator : MonoBehaviour
{
    public Animator animator;

    public GameObject defaultGeometry;
    
    public RenderSet[] renderSet;
    
    public void OnRender(ExecutionResult result)
    {
        if (!result.Success)
        {
            return;
        }

        if (result.BlueprintOutputs.ContainsKey("BodyNode") && result.BlueprintOutputs["BodyNode"] is Transform body) // The root graph contains an output with the name + type we are expecting
        {
            Debug.Log("We rendered a compatible body!", body);

            body.TryGetComponent<SkinnedMeshRenderer>(out var skin);
            
            foreach (var render in renderSet)
            {
                // TODO this will just apply the first render set, we need to differentiate between graphs
                // Either use instance id to get graph type and check that, OR check the gameobject name
                animator.avatar = render.avatar;
                RigUtilities.RetargetRig(render.rigRoot, skin);
                defaultGeometry?.SetActive(false);
                break;
            }
        }
        /*
        if (ctx.TryReadOutput("BodyNode", out Dynamic bodyDyn))
        {
            if (!bodyDyn.TryInterpretAs<Transform>(out Transform body))
            {
                Debug.LogError("Body node wrong type");
                return;
            }

            body.TryGetComponent<SkinnedMeshRenderer>(out var skin);

            var bundleProvider = ctx.GraphProvider as BundleGraphProvider;
            var graphId = bundleProvider.instanceTypeMap[ctx.InstanceId];
            var graph = bundleProvider.graphs[graphId];

            bool found = false;
            foreach (var render in renderSet)
            {
                if (graph.DisplayName != render.graphName) continue;
                
                animator.avatar = render.avatar;
                RigUtilities.RetargetRig(render.rigRoot, skin);
                defaultGeometry?.SetActive(false);
                found = true;
                break;
            }
            Debug.Log($"Found render set for graph {graph.DisplayName}? {found}");
        }*/
    }
    
    [System.Serializable]
    public class RenderSet
    {
        public string graphName;
        public Avatar avatar;
        public Transform rigRoot;
    }
}



