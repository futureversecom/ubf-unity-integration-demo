// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using Futureverse.FuturePass;
using Futureverse.Sylo;

namespace Futureverse.UBF.UBFExecutionController.Runtime
{
	public class FuturepassAuth : ISyloAuthDetails
	{
		public string GetAccessToken()
			=> FuturePassAuthentication.LoadedAuthenticationDetails?.AccessToken;
	}
}