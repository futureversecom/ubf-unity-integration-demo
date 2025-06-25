using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Futureverse.FuturePass
{
    public static class FuturePassAuthentication
    {

        public enum Environment
        {
            Development,
            Staging,
            Production
        }
        
        public static Environment CurrentEnvironment { get; private set; }
        public static CustodialAuthenticationResponse LoadedAuthenticationDetails { get; set; }

        private static CustodialHttpListener _listener;

        private static bool _autoCacheRefreshToken;

        private static string _currentState;
        private static string _currentCodeVerifier;
        
        private const string DevelopmentClientID = "ApfHakM-BwcErAkQupb6i";
        private const string StagingClientID = "ApfHakM-BwcErAkQupb6i";
        private const string ProductionClientID = "i8YTchXgUDYPswRfs3A5n";
        
        private const string ProductionBaseUrl = "https://login.pass.online";
        private const string StagingBaseUrl = "https://login.passonline.cloud";
        private const string DevelopmentBaseUrl = "https://login.passonline.cloud";
        
        private static string ClientID
        {
            get
            {
                switch (CurrentEnvironment)
                {
                    case Environment.Development:
                        return DevelopmentClientID;
                    case Environment.Staging:
                        return StagingClientID;
                    case Environment.Production:
                        return ProductionClientID;
                    default:
                        return "";
                }
            }
        }
        
        private static string BaseUrl
        {
            get
            {
                switch (CurrentEnvironment)
                {
                    case Environment.Development:
                        return DevelopmentBaseUrl;
                    case Environment.Staging:
                        return StagingBaseUrl;
                    case Environment.Production:
                        return ProductionBaseUrl;
                    default:
                        return "";
                }
            }
        }
        
        // Used to cache refresh token per environment
        private static string CacheKey => "Cached_Refresh_Token_" + CurrentEnvironment;
        
        private const string encKey = "D_y>r(xy3=,hD1-"; // This should be replaced with a non-hardcoded implementation
        private const string RedirectUri = "http://localhost:3000/callback";

        public static void SetEnvironment(Environment environment)
        {
            CurrentEnvironment = environment;
        }

        public static void SetTokenAutoCache(bool cacheAutomatically)
        {
            _autoCacheRefreshToken = cacheAutomatically;
        }
        
        /// <summary>
        /// Begin the custodial authentication flow. Opens a webpage and listens for a callback
        /// </summary>
        /// <param name="onSuccess">Authentication packet may be found in LoadedAuthenticationDetails</param>
        /// <param name="onFailure"></param>
        public static void StartLogin(Action onSuccess = null, Action<Exception> onFailure = null)
        {
            _currentState = GenerateSecureRandomString(128);
            _currentCodeVerifier = GenerateSecureRandomString(64);
            string codeChallenge = GenerateCodeChallenge(_currentCodeVerifier);
            
            _listener = new CustodialHttpListener
            {
                ExpectedState = _currentState
            };

            _listener.StartTokenAuthListener((authCode,state,expectedState) =>
            {
                CoroutineSceneObject.Instance.StartCoroutine(ParseAndExchangeCodeForCustodialResponseAsync(BaseUrl+"/", ClientID, _currentCodeVerifier, authCode, RedirectUri, () => {onSuccess?.Invoke();},
                    (exception) => { Debug.LogException(exception); onFailure?.Invoke(exception); }));
            });
            
            string nonce = GenerateSecureRandomString(128);
            Debug.Log("Logging in with URL: " + BaseUrl);
            Debug.Log("Client ID: " + ClientID);
            string authUrl = $"{BaseUrl}/auth?" +
                             "response_type=code" +
                             $"&client_id={ClientID}" +
                             $"&redirect_uri={HttpUtility.UrlEncode(RedirectUri)}" +
                             "&scope=openid" +
                             $"&code_challenge={codeChallenge}" +
                             "&code_challenge_method=S256" +
                             "&response_mode=query" +
                             "&prompt=login" +
                             $"&state={_currentState}" +
                             $"&nonce={nonce}";
            try
            {
                Application.OpenURL(authUrl);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error while requesting authorization code: " + ex.Message);
            }
        }
        
        /// <summary>
        /// Abort the custodial flow, closing the web listener
        /// </summary>
        public static void AbortLogin()
        {
            _listener.StopTokenAuthListener();
        }
        
        /// <summary>
        /// Use the authentication code provided from the custodial callback to request a full authentication packet
        /// </summary>
        /// <param name="baseUrl">The URL to the authentication API</param>
        /// <param name="clientID">The unique identifier of your client</param>
        /// <param name="codeVerifier">The random string used to create your auth code</param>
        /// <param name="authCode">The code returned from the custodial callback</param>
        /// <param name="redirectUri">The callback uri</param>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        private static IEnumerator ParseAndExchangeCodeForCustodialResponseAsync(string baseUrl, string clientID, string codeVerifier, string authCode, string redirectUri, Action onSuccess, Action<Exception> onError)
        {
            var body = $"grant_type=authorization_code" +
                       $"&code={authCode}" +
                       $"&redirect_uri={HttpUtility.UrlEncode(redirectUri)}" +
                       $"&client_id={clientID}" +
                       $"&code_verifier={codeVerifier}";

            string url = $"{baseUrl}token";
            
            var webRequest = new UnityWebRequest(url, "POST");
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            webRequest.uploadHandler.contentType = "application/x-www-form-urlencoded";
            webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error on token web request: {webRequest.error}");
                onError(new Exception(webRequest.error + "\n" + webRequest.downloadHandler.text));
                yield break;
            }
            
            var responseText = webRequest.downloadHandler.text;
            if (TryUpdateAuthentication(responseText, onError))
            {
                onSuccess?.Invoke();
            }
        }
        
        /// <summary>
        /// Begin the refresh flow to request new authentication details using an existing refresh token
        /// </summary>
        public static void RefreshToken()
        {
            CoroutineSceneObject.Instance.StartCoroutine(RefreshTokenRoutine(BaseUrl+"/", ClientID, LoadedAuthenticationDetails.RefreshToken));
        }
        
        /// <summary>
        /// Encrypt and store the currently loaded refresh token
        /// </summary>
        public static void CacheRefreshToken()
        {
            if (LoadedAuthenticationDetails == null)
            {
                PlayerPrefs.SetString(CacheKey, "");
                Debug.LogError("No loaded authentication details, erasing cached refresh token");
                return;
            }

            CacheRefreshToken(LoadedAuthenticationDetails.RefreshToken, encKey);
        }

        /// <summary>
        /// Encrypt and store a refresh token using a user-defined password
        /// </summary>
        /// <param name="refreshToken">The token to cache</param>
        /// <param name="passKey">The key used to encrypt the token, defaults to SDK standard</param>
        public static void CacheRefreshToken(string refreshToken, string passKey = encKey)
        {
            var basicEncryptedRefreshToken =
                EncryptionHandler.Encrypt(refreshToken, passKey);
            PlayerPrefs.SetString(CacheKey, basicEncryptedRefreshToken);
        }

        /// <summary>
        /// Load and decrypt a cached refresh token with a user-defined password
        /// </summary>
        /// <param name="passKey">Key used for decryption, defaults to SDK standard</param>
        public static void LoginFromCachedRefreshToken(string passKey = encKey)
        {
            string encryptedToken = PlayerPrefs.GetString(CacheKey);
            if (string.IsNullOrEmpty(encryptedToken))
            {
                Debug.LogError("No cached token found");
                return;
            }
            
            if (EncryptionHandler.TryDecrypt(encryptedToken, passKey, out var decryptedToken))
            {
                CoroutineSceneObject.Instance.StartCoroutine(RefreshTokenRoutine(BaseUrl+"/", ClientID, decryptedToken));
            }
            else
            {
                Debug.LogError("Unable to decrypt refresh token");
            }
        }
        
        /// <summary>
        /// Coroutine handling refresh token flow.
        /// </summary>
        /// <param name="baseUrl">The URL to the authentication API</param>
        /// <param name="clientID">The unique identifier of your client</param>
        /// <param name="refreshToken">The current valid refresh token</param>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        private static IEnumerator RefreshTokenRoutine(string baseUrl, string clientID, string refreshToken, Action onSuccess = null, Action<Exception> onError = null)
        {
            var body = $"grant_type=refresh_token" +
                       $"&refresh_token={refreshToken}" +
                       $"&client_id={clientID}";

            string url = $"{baseUrl}token";
            
            var webRequest = new UnityWebRequest(url, "POST");
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            webRequest.uploadHandler.contentType = "application/x-www-form-urlencoded";
            webRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error on token web request: {webRequest.error} : {webRequest.downloadHandler.text}");
                try
                {
                    var fvError = JsonConvert.DeserializeObject<FVError>(webRequest.downloadHandler.text);
                    if (fvError.Error == "invalid_grant")
                    {
                        Debug.LogError("Invalid grant request - ensure your cached refresh token is valid");
                    }
                }
                catch
                {
                    // The error was not a futureverse error; likely more generic like a 404 or 500 error.
                }
                onError?.Invoke(new Exception(webRequest.error + "\n" + webRequest.downloadHandler.text));
                yield break;
            }
            
            var responseText = webRequest.downloadHandler.text;
            if (TryUpdateAuthentication(responseText, onError))
            {
                onSuccess?.Invoke();
            }
        }

        /// <summary>
        /// Set the currently valid authentication packet, optionally caching the refresh token for later use
        /// </summary>
        /// <param name="responseText"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        private static bool TryUpdateAuthentication(string responseText, Action<Exception> onError = null)
        {
            CustodialAuthenticationResponse custodialResponse = null;
            try
            {
                custodialResponse = JsonConvert.DeserializeObject<CustodialAuthenticationResponse>(responseText);
                LoadedAuthenticationDetails = custodialResponse;
                if (_autoCacheRefreshToken)
                {
                    CacheRefreshToken();
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                onError?.Invoke(ex);
                return false;
            }
        }
        
        private static string GenerateSecureRandomString(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var data = new byte[length];
                rng.GetBytes(data);
                return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            }
        }

        private static string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').TrimEnd('=');
            }
        }

        /// <summary>
        /// Basic JSON structure of errors provided by the FuturePass API
        /// </summary>
        private class FVError
        {
            [JsonProperty("error")]
            public string Error;
            [JsonProperty("error_description")]
            public string ErrorDescription;
        }

        public static class EncryptionHandler
        {
            // Key size of the encryption algorithm in bits.
            private const int Keysize = 256;

            // Number of iterations for the password bytes generation function.
            private const int DerivationIterations = 1000;

            public static string Encrypt(string plainText, string passPhrase)
            {
                // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text so that the same Salt and IV values can be used when decrypting.  
                var saltStringBytes = Generate256BitsOfRandomEntropy();
                var ivStringBytes = Generate256BitsOfRandomEntropy();
                var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
                {
                    var keyBytes = password.GetBytes(Keysize / 8);
                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.BlockSize = 256;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                using (var cryptoStream =
                                       new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                                {
                                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                    cryptoStream.FlushFinalBlock();
                                    // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                    var cipherTextBytes = saltStringBytes;
                                    cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                    cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Convert.ToBase64String(cipherTextBytes);
                                }
                            }
                        }
                    }
                }
            }

            public static bool TryDecrypt(string cipherText, string passPhrase, out string decrypted)
            {
                if (string.IsNullOrEmpty(cipherText))
                {
                    decrypted = null;
                    return false;
                }

                // Get the complete stream of bytes that represent:
                // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
                var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
                // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
                var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
                // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
                var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
                // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
                var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2)
                    .Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

                bool success = true;
                try
                {
                    using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
                    {
                        var keyBytes = password.GetBytes(Keysize / 8);
                        using (var symmetricKey = new RijndaelManaged())
                        {
                            symmetricKey.BlockSize = 256;
                            symmetricKey.Mode = CipherMode.CBC;
                            symmetricKey.Padding = PaddingMode.PKCS7;
                            using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                            {
                                using (var memoryStream = new MemoryStream(cipherTextBytes))
                                {
                                    using (var cryptoStream =
                                           new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                                    using (var streamReader = new StreamReader(cryptoStream, Encoding.UTF8))
                                    {
                                        decrypted = streamReader.ReadToEnd();
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                    decrypted = null;
                    success = false;
                }

                return success;

            }

            private static byte[] Generate256BitsOfRandomEntropy()
            {
                var randomBytes = new byte[32]; // 256 bits
                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    rngCsp.GetBytes(randomBytes);
                }

                return randomBytes;
            }
        }
    }
}
