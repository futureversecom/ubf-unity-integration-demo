// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using Futureverse.Sylo;
using Futureverse.UBF.Runtime;
using Futureverse.UBF.Runtime.Builtin;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Resources;
using GLTFast;
using UnityEngine;
using UnityEngine.Networking;
using IDownloader = Futureverse.UBF.Runtime.Resources.IDownloader;

namespace Futureverse.UBF.UBFExecutionController.Runtime
{
    public class SyloDownloader : IDownloader
    {
        private readonly ISyloAuthDetails _auth = new FuturepassAuth();

		public IEnumerator DownloadBytes(string uri, Action<byte[]> onComplete)
        {
            return uri.StartsWith("did:") ? 
				SyloUtilities.GetBytesFromDID(uri, _auth, onComplete, _ => onComplete?.Invoke(null)) : 
				WebRequestDownload(uri, onComplete);
        }

        private IEnumerator WebRequestDownload(string uri, Action<byte[]> onComplete)
        {
            var normalizedUri = UriUtils.NormalizeUri(uri);
            if (normalizedUri == null)
            {
                onComplete?.Invoke(null);
                yield break;
            }
				
            var request = UnityWebRequest.Get(normalizedUri);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download from {normalizedUri}\nResult: {request.responseCode} - {request.result}");

                onComplete?.Invoke(null);
                yield break;
            }

            var bytes = request.downloadHandler.data;
            onComplete?.Invoke(bytes);
        }
    }
    
    public class FutureverseArtifactProvider : ArtifactProvider
    {
	    private readonly SyloDownloader _downloader = new();

		public override IEnumerator GetTextureResource(
       		ResourceId resourceId,
       		TextureImportSettings settings,
       		Action<Texture2D, TextureAssetImportSettings> onComplete)
       	{
       		var loader = new TextureLoader();
       		loader.SetSrgb(settings.UseSrgb);

	        return GetResource(
		        resourceId,
		        ResourceType.Texture,
		        onComplete,
		        loader,
		        _downloader
	        );
        }
   
       	public override IEnumerator GetBlueprintResource(ResourceId resourceId, string instanceId, Action<Blueprint, BlueprintAssetImportSettings> onComplete)
       	{
       		var loader = new BlueprintLoader();
       		loader.SetInstanceId(instanceId);

	        return GetResource(
		        resourceId,
		        ResourceType.Blueprint,
		        onComplete,
		        loader,
		        _downloader
	        );
       	}
   
       	public override IEnumerator GetMeshResource(ResourceId resourceId, Action<GltfImport, MeshAssetImportSettings> onComplete)
        {
	        return GetResource<GltfImport, GltfLoader, MeshAssetImportSettings, SyloDownloader>(
		        resourceId,
		        ResourceType.Mesh,
		        onComplete,
		        downloader:_downloader
	        );
        }
    }
}