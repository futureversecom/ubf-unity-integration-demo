using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Futureverse.FuturePass;

namespace Futureverse.Sylo
{
    public class FuturePassAuthDetails : ISyloAuthDetails
    {
        public string GetAccessToken()
        {
            var auth = FuturePassAuthentication.LoadedAuthenticationDetails;
            if (auth == null)
            {
                return null;
            }

            return auth.AccessToken;
        }
    }
}
