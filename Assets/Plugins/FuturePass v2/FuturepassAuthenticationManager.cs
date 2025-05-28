using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Security.Cryptography;
using Newtonsoft.Json;
public class FuturepassAuthenticationManager : MonoBehaviour
{

    
    /// <summary>
    /// Generates a secure random string of the specified length.
    /// </summary>
    /// <param name="length">The length of the random string.</param>
    /// <returns>A secure random string.</returns>
    private string GenerateSecureRandomString(int length)
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            var data = new byte[length];
            rng.GetBytes(data);
            return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }

    /// <summary>
    /// Generates a code challenge based on the provided code verifier.
    /// </summary>
    /// <param name="codeVerifier">The code verifier used to create the challenge.</param>
    /// <returns>The generated code challenge.</returns>
    private string GenerateCodeChallenge(string codeVerifier)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
    
    public class CustodialLoginManager
    {
        public CustodialAccessTokenResponse CachedAccessTokenResponse { get; set; }

        private string currentState;
        private string currentCodeVerifier;

        // Need to add support for Developer Custom URLs, lets use a serialised object.
        private string DevelopmentClientID = "3KMMFCuY59SA4DDV8ggwc";
        private string StagingClientID = "3KMMFCuY59SA4DDV8ggwc";
        private string ProductionClientID = "G9mOSDHNklm_dCN0DHvfX";
        private string ProductionBaseUrl = "https://login.pass.online";
        private string StagingBaseUrl = "https://login.passonline.cloud";
        
        private const string RedirectUri = "http://localhost:3000/callback";
        
        public void StartCustodialAuthFlow()
        {

        }
        
        public class CustodialAccessTokenResponse
        {
            [JsonProperty("access_token")]
            public string Token { get; set; }
            
            [JsonProperty("id_token")]
            public string RawIdToken
            {
                get => rawToken;
                set
                {
                    rawToken = value;
                    DecodedToken = DecodeIdToken(value);
                } 
            }

            private string rawToken;

            [JsonIgnore] 
            public CustodialIdToken DecodedToken;

            [JsonProperty("expires_in")]
            private int ExpiresInSeconds { get; set; }

            [JsonIgnore]
            public DateTime Expiration => DateTime.UtcNow.AddSeconds(ExpiresInSeconds);

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            private CustodialIdToken DecodeIdToken(string rawToken)
            {
                // Split the JWT into its components
                var parts = rawToken.Split('.');
                if (parts.Length < 2)
                {
                    throw new FormatException("Invalid ID token format.");
                }

                // Decode the payload (second part of the JWT)
                var payload = parts[1];
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                var idTokenBytes = Convert.FromBase64String(payload);
                var custodialToken = JsonConvert.DeserializeObject<CustodialIdToken>(Encoding.UTF8.GetString(idTokenBytes));
                return custodialToken;
            }
        }
    
        /// <summary>
        /// Represents the payload of the ID token from the Futureverse authentication.
        /// </summary>
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
        
        public static class CustodialLocalWebServerHelper
        {
            private static HttpListener httpListener;
            private static bool isTokenAuthListenerStarted = false;
            private static bool isSigningServerStarted = false;
            private const int ServerPort = 3000;
            private const string CallbackPath = "/callback";
            
            private const string Base64Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

            /// <summary>
            /// The expected state value for validating CSRF protection.
            /// </summary>
            public static string ExpectedState { get; set; }

            /// <summary>
            /// Starts a local web server to listen for the OAuth callback.
            /// </summary>
            /// <param name="onAuthCodeReceived">Callback function to handle the authorization code and state.</param>
            public static void StartTokenAuthListener(Action<string, string, string> onAuthCodeReceived)
            {
                if (isTokenAuthListenerStarted)
                {
                    Debug.Log("Local web server is already started.");
                    return;
                }

                httpListener = new HttpListener();
                httpListener.Prefixes.Add($"http://localhost:{ServerPort}{CallbackPath}/");
                httpListener.Start();
                isTokenAuthListenerStarted = true;

                // Start listening for requests asynchronously
                Task.Run(() => ListenForTokenAuthRequests(onAuthCodeReceived));
            }

            /// <summary>
            /// Stops the local web server.
            /// </summary>
            public static void StopTokenAuthListener()
            {
                if (isTokenAuthListenerStarted)
                {
                    httpListener.Stop();
                    httpListener.Close();
                    isTokenAuthListenerStarted = false;
                    Debug.Log("Local web server stopped.");
                }
            }

            /// <summary>
            /// Listens for incoming requests and handles the OAuth callback.
            /// </summary>
            /// <param name="onAuthCodeReceived">Callback function to handle the authorization code and state.</param>
            private static async Task ListenForTokenAuthRequests(Action<string, string, string> onAuthCodeReceived)
            {
                while (isTokenAuthListenerStarted)
                {
                    try
                    {
                        var context = await httpListener.GetContextAsync();
                        var request = context.Request;

                        if (request.HttpMethod == "GET" && request.Url.AbsolutePath == CallbackPath)
                        {
                            var queryParams = request.QueryString;
                            string authCode = queryParams["code"];
                            string state = queryParams["state"];

                            if (string.IsNullOrEmpty(authCode))
                            {
                                Debug.LogError("Authorization code not found in the request.");
                                return;
                            }

                            StopTokenAuthListener();
                            onAuthCodeReceived?.Invoke(authCode, state, ExpectedState);
                        }
                    }
                    catch (HttpListenerException ex)
                    {
                        Debug.LogError("HttpListener exception: " + ex.Message);
                    }
                }
            }
        
            /// <summary>
            /// Starts a local web server to listen for the Signing Callbacks.
            /// </summary>
            /// <param name="onSigningResponseReceived">Callback function to handle the signing response.</param>
            public static void StartSigningServer(string callbackPath, Action<string> onSigningResponseReceived)
            {
                if (isSigningServerStarted)
                {
                    Debug.LogWarning("Local web server is already started.");
                    return;
                }

                httpListener = new HttpListener();
                httpListener.Prefixes.Add($"http://localhost:{ServerPort}/{callbackPath}/");
                httpListener.Start();
                isSigningServerStarted = true;


                // Start listening for requests asynchronously
                Task.Run(() => ListenForSigningRequests(callbackPath, onSigningResponseReceived));
            }

            /// <summary>
            /// Listens for incoming Signature response callback.
            /// </summary>
            /// <param name="callbackPath"></param>
            /// <param name="onSigningResponseReceived"></param>
            private static async Task ListenForSigningRequests(string callbackPath, Action<string> onSigningResponseReceived)
            {
                while (isSigningServerStarted)
                {
                    try
                    {
                        var context = await httpListener.GetContextAsync();
                        var request = context.Request;

                        if (request.HttpMethod == "GET" && request.Url.AbsolutePath == $"/{callbackPath}")
                        {
                            string responseJson = request.QueryString["response"];
                        
                            if (!string.IsNullOrEmpty(responseJson))
                            {
                                onSigningResponseReceived?.Invoke(Encoding.UTF8.GetString(ConvertFromBase64String(responseJson)));
                                StopSigningServer();
                            }
                            else
                            {
                                Debug.LogError("No response message received.");
                            }
                        }
                    }
                    catch (HttpListenerException ex)
                    {
                        Debug.LogError("HttpListener exception: " + ex.Message);
                    }
                }
            }
        
            
            /// <summary>
            /// Terminates signing server activity and closes the local HTTP Listener.
            /// </summary>
            public static void StopSigningServer()
            {
                if (isSigningServerStarted)
                {
                    httpListener.Stop();
                    httpListener.Close();
                    isSigningServerStarted = false;
                }
            }
            
            /// <summary>
            /// Custom function to handle bit shifting for Base 64 Conversion, Necessary as the .Net function caused failures.
            /// </summary>
            /// <param name="base64">The Base64 string to be converted.</param>
            /// <returns></returns>
            public static byte[] ConvertFromBase64String(string base64)
            {
                base64 = base64.TrimEnd('=');
                int padding = base64.Length % 4;
                if (padding > 0)
                {
                    base64 += new string('=', 4 - padding);
                }

                byte[] bytes = new byte[base64.Length * 3 / 4];
                int byteIndex = 0;

                for (int i = 0; i < base64.Length; i += 4)
                {
                    int b1 = Base64Characters.IndexOf(base64[i]);
                    int b2 = Base64Characters.IndexOf(base64[i + 1]);
                    int b3 = Base64Characters.IndexOf(base64[i + 2]);
                    int b4 = Base64Characters.IndexOf(base64[i + 3]);

                    bytes[byteIndex++] = (byte)((b1 << 2) | (b2 >> 4));
                    if (b3 != -1)
                        bytes[byteIndex++] = (byte)(((b2 & 0x0F) << 4) | (b3 >> 2));
                    if (b4 != -1)
                        bytes[byteIndex++] = (byte)(((b3 & 0x03) << 6) | b4);
                }

                return bytes;
            }
        }
    }
}

    
    