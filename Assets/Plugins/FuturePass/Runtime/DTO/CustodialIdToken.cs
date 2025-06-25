using System;
using Newtonsoft.Json;

namespace Futureverse.FuturePass
{
    [Serializable]
    public class CustodialIdToken
    {
        /// <summary>
        /// The subject identifier.
        /// </summary>
        [JsonProperty("sub")]
        public string Sub { get; set; }

        /// <summary>
        /// The Ethereum address.
        /// </summary>
        [JsonProperty("eoa")]
        public string Eoa { get; set; }

        /// <summary>
        /// The custodian information.
        /// </summary>
        [JsonProperty("custodian")]
        public string Custodian { get; set; }

        /// <summary>
        /// The chain ID.
        /// </summary>
        [JsonProperty("chainId")]
        public int ChainId { get; set; }

        /// <summary>
        /// The user email.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// The futurepass string.
        /// </summary>
        [JsonProperty("futurepass")]
        public string Futurepass { get; set; }

        /// <summary>
        /// The authentication time (Unix timestamp).
        /// </summary>
        [JsonProperty("auth_time")]
        public long AuthTime { get; set; }

        /// <summary>
        /// The nonce for replay protection.
        /// </summary>
        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        /// <summary>
        /// The access token hash.
        /// </summary>
        [JsonProperty("at_hash")]
        public string AtHash { get; set; }

        /// <summary>
        /// The audience (client ID).
        /// </summary>
        [JsonProperty("aud")]
        public string Aud { get; set; }

        /// <summary>
        /// The expiration time (Unix timestamp).
        /// </summary>
        [JsonProperty("exp")]
        public long Exp { get; set; }

        /// <summary>
        /// The issued at time (Unix timestamp).
        /// </summary>
        [JsonProperty("iat")]
        public long Iat { get; set; }

        /// <summary>
        /// The issuer URL.
        /// </summary>
        [JsonProperty("iss")]
        public string Iss { get; set; }
    }
}

