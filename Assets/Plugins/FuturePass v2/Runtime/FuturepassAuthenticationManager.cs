using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;
using System.Web;
using Newtonsoft.Json;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(FuturepassAuthenticationManager))]
public class FuturepassAuthenticationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        var mg = (FuturepassAuthenticationManager)target;
        if (GUILayout.Button("Start Login"))
        {
            mg.StartLogin();
        }

        if (GUILayout.Button("Abort Login"))
        {
            mg.AbortLogin();
        }

        if (GUILayout.Button("Refresh Token"))
        {
            mg.RefreshToken();   
        }

        if (GUILayout.Button("Cache Refresh Token"))
        {
            mg.CacheRefreshToken();
        }

        if (GUILayout.Button("Login From Cached Token"))
        {
            mg.LoginFromCachedRefreshToken();
        }

        EditorGUILayout.Space();
        
        if (mg.LoadedAuthenticationDetails != null)
        {
            string json = JsonConvert.SerializeObject(mg.LoadedAuthenticationDetails, Formatting.Indented);
            EditorGUILayout.TextArea(json);
        }
        else
        {
            EditorGUILayout.TextArea("\n\n");
        }
    }
}
#endif


public class FuturepassAuthenticationManager : MonoBehaviour
{
    public enum Environment
    {
        Development,
        Staging,
        Production
    }
    
    private CustodialHttpListener _listener;
    public CustodialAuthenticationResponse LoadedAuthenticationDetails { get; set; }

    public Environment CurrentEnvironment;
    public bool cacheRefreshToken = true;
    
    private string _currentState;
    private string _currentCodeVerifier;

    private const string DevelopmentClientID = "ApfHakM-BwcErAkQupb6i";
    private const string StagingClientID = "ApfHakM-BwcErAkQupb6i";
    private const string ProductionClientID = "i8YTchXgUDYPswRfs3A5n";
    private const string ProductionBaseUrl = "https://login.pass.online";
    private const string StagingBaseUrl = "https://login.passonline.cloud";
    private const string DevelopmentBaseUrl = "https://login.passonline.cloud";

    private const string encKey = "D_y>r(xy3=,hD1-"; // This should be replaced with a non-hardcoded implementation
    
    private string ClientID
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
    
    private string BaseUrl
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
    
    private const string RedirectUri = "http://localhost:3000/callback";
    
    public void StartLogin(Action onSuccess = null, Action onFailure = null)
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
            StartCoroutine(ParseAndExchangeCodeForCustodialResponseAsync(BaseUrl+"/", ClientID, _currentCodeVerifier, authCode, RedirectUri, () => {onSuccess?.Invoke();},
                (exception) => { Debug.LogException(exception); onFailure?.Invoke(); }));
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
    
    public void AbortLogin()
    {
        _listener.StopTokenAuthListener();
    }
    
    private IEnumerator ParseAndExchangeCodeForCustodialResponseAsync(string baseUrl, string clientID, string codeVerifier, string authCode, string redirectUri, Action onSuccess, Action<Exception> onError)
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
    
    public void RefreshToken()
    {
        StartCoroutine(RefreshTokenRoutine(BaseUrl+"/", ClientID, LoadedAuthenticationDetails.RefreshToken));
    }

    public void CacheRefreshToken()
    {
        if (LoadedAuthenticationDetails == null)
        {
            PlayerPrefs.SetString("Cached_Refresh_Token", "");
            Debug.LogError("No loaded authentication details, erasing cached refresh token");
            return;
        }
        var basicEncryptedRefreshToken =
            EncryptionHandler.Encrypt(LoadedAuthenticationDetails?.RefreshToken, encKey);
        PlayerPrefs.SetString("Cached_Refresh_Token", basicEncryptedRefreshToken);
    }

    public void LoginFromCachedRefreshToken()
    {
        string encryptedToken = PlayerPrefs.GetString("Cached_Refresh_Token");
        if (string.IsNullOrEmpty(encryptedToken))
        {
            Debug.LogError("No cached token found");
            return;
        }
        
        if (EncryptionHandler.TryDecrypt(encryptedToken, encKey, out var decryptedToken))
        {
            StartCoroutine(RefreshTokenRoutine(BaseUrl+"/", ClientID, decryptedToken));
        }
        else
        {
            Debug.LogError("Unable to decrypt refresh token");
        }
    }
    
    private IEnumerator RefreshTokenRoutine(string baseUrl, string clientID, string refreshToken, Action onSuccess = null, Action<Exception> onError = null)
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

    private bool TryUpdateAuthentication(string responseText, Action<Exception> onError = null)
    {
        CustodialAuthenticationResponse custodialResponse = null;
        try
        {
            custodialResponse = JsonConvert.DeserializeObject<CustodialAuthenticationResponse>(responseText);
            LoadedAuthenticationDetails = custodialResponse;
            if (cacheRefreshToken)
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
    
    private static string ConvertToHex(string message)
    {
        StringBuilder hex = new StringBuilder(message.Length * 2);
        foreach (char c in message)
        {
            hex.AppendFormat("{0:X2}", (int)c);
        }
        return hex.ToString();
    }
    
    private string GenerateSecureRandomString(int length)
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            var data = new byte[length];
            rng.GetBytes(data);
            return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }

    private string GenerateCodeChallenge(string codeVerifier)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Convert.ToBase64String(hash).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }

    private class FVError
    {
        [JsonProperty("error")]
        public string Error;
        [JsonProperty("error_description")]
        public string ErrorDescription;
    }
    
    public static class EncryptionHandler
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
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
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
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
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

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
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            using (var streamReader = new StreamReader(cryptoStream, Encoding.UTF8))
                            {
                                decrypted = streamReader.ReadToEnd();
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
