// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Testbed.AssetRegister
{
    public static class AssetRegisterQuery
    {
       private static readonly Dictionary<string, InventoryNode[]> s_walletQueryResultCache = new();

       public static IEnumerator InventoryQueryRoutine(string walletAddress, string[] collectionIds, int numResults, Action<bool, InventoryNode[]> callback)
       {
          yield return InventoryQueryRoutine(new[] {walletAddress}, collectionIds, numResults, callback);
       }
       
       public static IEnumerator InventoryQueryRoutine(string[] walletAddresses, string[] collectionIds, int numResults, Action<bool, InventoryNode[]> callback)
       {
          if (walletAddresses == null || walletAddresses.Length == 0)
          {
             Debug.LogError("InventoryQueryRoutine: no walletAddresses provided.");
             callback?.Invoke(false, null);
             yield break;
          }

          // gracefully handle empty/duplicate wallet addresses
          string[] validAddresses = walletAddresses.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct().ToArray();

          if (validAddresses.Length == 0)
          {
             Debug.LogError("InventoryQueryRoutine: no walletAddresses provided.");
             callback?.Invoke(false, null);
             yield break;
          }
          
          walletAddresses = validAddresses;
          
          const string query = @"
            query Assets($addresses: [ChainAddress!]!, $collectionIds: [CollectionId!], $first: Float) {
              assets(addresses: $addresses, collectionIds: $collectionIds, first: $first) {
                edges {
                  node {
                    id
                    collectionId
                    tokenId
                    assetType
                    metadata {
                      attributes
                      properties
                      uri
                      rawAttributes
                    }
                    collection {
                      chainId
                      chainType
                    }
                    assetTree {
                      data
                    }
                  }
                }
              }
            }";

          InventoryQueryVariables variables = new InventoryQueryVariables { Addresses = walletAddresses, CollectionIds = collectionIds, First = numResults };

          var payload = new { query, variables };
          string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.None);

          using UnityWebRequest webRequest = new UnityWebRequest("https://ar-api.futureverse.app/graphql", "POST");
          
          webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonPayload));
          webRequest.downloadHandler = new DownloadHandlerBuffer();
          webRequest.SetRequestHeader("Content-Type", "application/json");

          yield return webRequest.SendWebRequest();

          if (webRequest.result != UnityWebRequest.Result.Success)
          {
             Debug.LogError($"GraphQL request failed: {webRequest.error}");
             callback?.Invoke(false, null);
             
             yield break;
          }

          string resultString = webRequest.downloadHandler.text;
          InventoryResultData resultData = JsonConvert.DeserializeObject<InventoryResultData>(resultString);
          var nodes = resultData.InventoryData.InventoryAssets.Edges.Select(e => e.InventoryNode).ToArray();

          callback?.Invoke(true, nodes);
       }
    }
}