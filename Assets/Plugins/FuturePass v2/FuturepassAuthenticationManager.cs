using System;
using System.Collections;
using System.Collections.Generic;
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

        var mg = (FuturepassAuthenticationManager)target;
        if (GUILayout.Button("Start Login"))
        {
            mg.StartLogin();
        }

        if (GUILayout.Button("Abort Login"))
        {
            mg.AbortLogin();
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
    public CustodialAccessTokenResponse CachedAccessTokenResponse { get; set; }

    public Environment CurrentEnvironment;
    
    private string _currentState;
    private string _currentCodeVerifier;

    private const string DevelopmentClientID = "3KMMFCuY59SA4DDV8ggwc";
    private const string StagingClientID = "3KMMFCuY59SA4DDV8ggwc";
    private const string ProductionClientID = "G9mOSDHNklm_dCN0DHvfX";
    private const string ProductionBaseUrl = "https://login.pass.online";
    private const string StagingBaseUrl = "https://login.passonline.cloud";
    private const string DevelopmentBaseUrl = "https://login.passonline.cloud";
    
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
    
    public void StartLogin()
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
            StartCoroutine(ParseAndExchangeCodeForCustodialResponseAsync(BaseUrl+"/", ClientID, _currentCodeVerifier, authCode, RedirectUri, (
                accessTokenResponse => { CachedAccessTokenResponse = accessTokenResponse;}), Debug.LogException));
        });
        
        string nonce = GenerateSecureRandomString(128);

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
    
    private IEnumerator ParseAndExchangeCodeForCustodialResponseAsync(string baseUrl, string clientID, string codeVerifier, string authCode, string redirectUri, Action<CustodialAccessTokenResponse> onSuccess, Action<Exception> onError)
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
        CustodialAccessTokenResponse custodialResponse = null;
        try
        {
            custodialResponse = JsonConvert.DeserializeObject<CustodialAccessTokenResponse>(responseText);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            onError?.Invoke(ex);
            yield break;
        }
        onSuccess?.Invoke(custodialResponse);
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
}
