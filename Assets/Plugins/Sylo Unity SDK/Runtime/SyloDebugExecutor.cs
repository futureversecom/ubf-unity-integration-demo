using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;

namespace Futureverse.Sylo
{
    [CustomEditor(typeof(SyloDebugExecutor))]
    public class SyloDebugExecutorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Run Debug"))
            {
                (target as SyloDebugExecutor)?.RunDebug();
            }
        }
    }
}
#endif

namespace Futureverse.Sylo
{
    public class SyloDebugExecutor : MonoBehaviour
    {
        public string debug_did;
        public string debug_resolverUri;
        [Header("Requires new value whenever token is refreshed")]
        public string debug_accessToken;

        private void Start()
        {
            SyloUtilities.SetResolverUri(debug_resolverUri);
        }

        public void RunDebug()
        {
            StartCoroutine(SyloUtilities.GetBytesFromDID(debug_did, new DebugAuthDetails(debug_accessToken), bytes => Debug.Log($"Received {bytes.Length} bytes"), Debug.LogException));
        }
    }

    public class DebugAuthDetails : ISyloAuthDetails
    {
        private readonly string _accessToken;
        public DebugAuthDetails(string accessToken)
        {
            _accessToken = accessToken;
        }
        
        public string GetAccessToken()
        {
            return _accessToken;
        }
    }
}

