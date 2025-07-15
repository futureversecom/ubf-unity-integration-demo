// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using Futureverse.Sylo;
using Futureverse.UBF.Runtime.Resources;

namespace Futureverse.UBF.UBFExecutionController.Runtime
{
    public class SyloDownloader : IDownloader
    {
        private readonly ISyloAuthDetails _auth = new FuturepassAuth();

        public bool CanDownload(IResourceData resource)
            => resource.Uri.StartsWith("did:");

        public IEnumerator DownloadBytes(string url, Action<byte[]> onComplete)
            => SyloUtilities.GetBytesFromDID(
                url,
                _auth,
                onComplete,
                _ => onComplete?.Invoke(null)
            );
    }
}