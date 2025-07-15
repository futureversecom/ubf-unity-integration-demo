// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine.Networking;

namespace Futureverse.UBF.Runtime.Resources
{
	/// <summary>
	/// Mechanism for specifying how a given resource should be downloaded.
	/// </summary>
	public interface IDownloader
	{
		bool CanDownload(IResourceData resource);
		/// <summary>
		/// Coroutine that downloads raw bytes from a uri.
		/// </summary>
		/// <param name="uri">The location of the bytes to download</param>
		/// <param name="onComplete">Callback containing the loaded bytes</param>
		/// <returns>IEnumerator to yield on.</returns>
		IEnumerator DownloadBytes(string uri, Action<byte[]> onComplete);
	}

	/// <summary>
	/// Default implementation of the IDownloader interface. Uses UnityWebRequest to download bytes from the given URI
	/// </summary>
	public class DefaultDownloader : IDownloader
	{
		public bool CanDownload(IResourceData resource)
			=> true;
		
		public IEnumerator DownloadBytes(string uri, Action<byte[]> onComplete)
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
				UbfLogger.LogError($"Failed to download from {normalizedUri}\nResult: {request.responseCode} - {request.result}");

				onComplete?.Invoke(null);
				yield break;
			}

			var bytes = request.downloadHandler.data;
			onComplete?.Invoke(bytes);
		}
	}
}