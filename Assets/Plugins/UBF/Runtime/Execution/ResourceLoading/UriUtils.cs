// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.IO;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Resources
{
	public static class UriUtils
	{
		public static string NormalizeUri(string uriString)
		{
			if (string.IsNullOrWhiteSpace(uriString))
			{
				Debug.LogWarning("URI cannot be null, or contain whitespace.");
				return null;
			}

			if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
			{
				if (uri.Scheme == Uri.UriSchemeHttp ||
					uri.Scheme == Uri.UriSchemeHttps ||
					uri.Scheme == Uri.UriSchemeFile)
				{
					return uri.AbsoluteUri;
				}
			}
			
			try
			{
				var fileUri = new Uri(Path.GetFullPath(uriString));
				return fileUri.AbsoluteUri;
			}
			catch (Exception)
			{
				Debug.LogWarning("URI is not a valid local path.");
				return null;
			}
		}
	}
}