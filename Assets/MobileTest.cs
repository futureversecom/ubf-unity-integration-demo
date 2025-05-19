using System;
using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime;
using Futureverse.UBF.Runtime.Builtin;
using Futureverse.UBF.Runtime.Execution;
using GLTFast;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class MobileTest : MonoBehaviour
{
    [TextArea(10,100)]
    public string json;

    public TMP_Text text;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Execute());
    }

    IEnumerator Execute()
    {
        var ap = new MTArtifactProvider();
        var instance = new BlueprintInstanceData("graph");
        var executionData = new ExecutionData(
            transform,
            OnExecutionFinished,
            new List<IBlueprintInstanceData>() { instance },
            ap
        );
        StartCoroutine(UBFExecutor.ExecuteRoutine(executionData, instance.InstanceId));
        yield break;
    }

    void OnExecutionFinished(ExecutionResult result)
    {
        text.text = JsonConvert.SerializeObject(result.Success + "\n" + result.BlueprintOutputs);
    }
}

public class MTArtifactProvider : IArtifactProvider
{
    public string JSON;
    public Blueprint BP;

    public void RegisterJSON(string json)
    {
        JSON = json;
        Blueprint.TryLoad("graph", json, out BP);
    }
    
    public IEnumerator GetTextureResource(ResourceId resourceId, TextureImportSettings settings, Action<Texture2D> onComplete)
    {
        throw new NotImplementedException();
    }

    public IEnumerator GetBlueprintResource(ResourceId resourceId, string instanceId, Action<Blueprint> onComplete)
    {
        onComplete?.Invoke(BP);
        yield break;
    }

    public IEnumerator GetMeshResource(ResourceId resourceId, Action<GltfImport> onComplete)
    {
        throw new NotImplementedException();
    }
}
