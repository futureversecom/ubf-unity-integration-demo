using System;
using System.Collections;
using System.Collections.Generic;
using Futureverse.Sylo;
using UnityEngine;
using UnityEngine.Networking;

namespace Futureverse.Sylo
{
    public static class SyloUtilities
    {
        public static string ResolverUri { get; private set; }

        public static void SetResolverUri(string uri)
        {
            ResolverUri = uri;
        }
        
        public static IEnumerator GetBytesFromDID(string did, ISyloAuthDetails authDetails, Action<byte[]> onSuccess, Action<Exception> onError = null)
        {
            if (string.IsNullOrEmpty(did))
            {
                yield break;
            }

            if (!TryParseDID(did, out var futurePassAddress, out var dataId))
            {
                yield break;
            }
                
            string uri = ResolverUri; // TODO: We need api to discover sylo/resolvers
            uri += "/api/v1/objects/get/" + futurePassAddress + "/" + dataId + "?authType=access_token";
                
            var webRequest = UnityWebRequest.Get(uri);
            webRequest.SetRequestHeader("Accept", "*/*");
            webRequest.SetRequestHeader("Authorization", "Bearer " + authDetails.GetAccessToken());
                
            yield return webRequest.SendWebRequest();
            Debug.Log("Result: " + webRequest.result);
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(new Exception($"WR Err: Result: {webRequest.result}, Code: {webRequest.responseCode}, Error: {webRequest.error}"));
                yield break;
            }
                
            onSuccess?.Invoke(webRequest.downloadHandler.data);
            Debug.Log(webRequest.downloadHandler.text);
            yield break;
        }
            
        public static bool TryParseDID(string did, out string futurePassAddress, out string dataId)
        {
            futurePassAddress = null;
            dataId = null;
                
            // did:sylo-data:futurepassID/data id
                
            var split = did.Split(':');
            if (split == null || split.Length == 0 || split[0] != "did")
            {
                return false;
            }
                
            var dataSplit = split[2].Split('/');
            futurePassAddress = dataSplit[0];
            dataId = dataSplit[1];
                
            return true;
        }
    }
}

