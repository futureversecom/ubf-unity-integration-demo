// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace Testbed.AssetRegister
{
	public static class AssetRegisterQuery
	{
    private static readonly Dictionary<string, InventoryNode[]> s_walletQueryResultCache = new();
    private const string QueryUri = "https://ar-api.futureverse.app/graphq";

    public static IEnumerator InventoryQueryRoutine(string walletAddress, string[] collectionIds, int numResults, Action<bool, InventoryNode[]> callback, string uri = QueryUri)
    {
      if (s_walletQueryResultCache.TryGetValue(walletAddress, out var nodes))
      {
        callback?.Invoke(true, nodes);
        yield break;
      }
      
      const string query = @"query Assets($addresses: [ChainAddress!]!, $collectionIds: [CollectionId!], $first: Float) {
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
      
      var variables = new InventoryQueryVariables() { Addresses = new []{walletAddress}, CollectionIds = collectionIds, First = numResults, };
      var q = new
      {
        query,
        variables,
      };

      var queryJson = JsonConvert.SerializeObject(q, Formatting.None);
      var webRequest = new UnityWebRequest(uri, "POST");
      webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(queryJson));
      webRequest.SetRequestHeader("Content-Type", "application/json");
      webRequest.downloadHandler = new DownloadHandlerBuffer();
      yield return webRequest.SendWebRequest();

      if (webRequest.result != UnityWebRequest.Result.Success)
      {
        Debug.LogError(webRequest.error);
        callback?.Invoke(false, null);
        yield break;
      }

      var resultString = Encoding.UTF8.GetString(webRequest.downloadHandler.data);
      var resultData = JsonConvert.DeserializeObject<InventoryResultData>(resultString);
      nodes = resultData.InventoryData.InventoryAssets.Edges.Select(e => e.InventoryNode).ToArray();
      s_walletQueryResultCache.Add(walletAddress, nodes);
      callback?.Invoke(true, nodes);
    }
	}
}