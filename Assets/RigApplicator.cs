using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime;
using Futureverse.UBF.Runtime.Execution;
using UnityEngine;

public class RigApplicator : MonoBehaviour
{
    public Animator animator;

    public GameObject defaultGeometry;
    
    public void OnRender(ExecutionResult result)
    {
        if (!result.Success)
        {
            return;
        }

        if (result.BlueprintOutputs.ContainsKey("BodyNode") && result.BlueprintOutputs["BodyNode"] is Transform body) // The root graph contains an output with the name + type we are expecting
        {
            defaultGeometry?.SetActive(false);
        }
    }
}



