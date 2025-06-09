using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SyloExecutor))]
public class SyloExecutorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Run"))
        {
            (target as SyloExecutor).RunTest();
        }
    }
}
#endif
public class SyloExecutor : MonoBehaviour
{
    public string futurepassId;
    public string resolverId;
    public string resolverUri;
    public string dataId;
    public string accessToken;
    
    public void RunTest()
    {
        StartCoroutine(SyloGetTestRoutine());
    }

    IEnumerator SyloGetTestRoutine()
    {
        var webRequest = UnityWebRequest.Get(GetURI());
        webRequest.SetRequestHeader("Accept", "*/*");
        webRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
        yield return webRequest.SendWebRequest();
        Debug.Log("Result: " + webRequest.result);
        Debug.Log(webRequest.downloadHandler.text);
        yield break;
    }

    private string GetURI()
    {
        string uri = resolverUri;
        uri += "/api/v1/objects/get/" + futurepassId + "/" + dataId + "?authType=access_token";
        return uri;
    }
}
