using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime;
using Futureverse.UBF.Runtime.Execution;
using UnityEngine;

/// <summary>
/// This class was previously used for retargeting the rig, before MeshConfig was implemented. Now, it is used to disable the existing geometry after the graph has finished execution
/// </summary>
public class RigApplicator : MonoBehaviour
{
    public GameObject defaultGeometry;
    
    public void OnRender(ExecutionResult result)
    {
        if (!result.Success)
        {
            return;
        }

        defaultGeometry.SetActive(false);
    }
}



