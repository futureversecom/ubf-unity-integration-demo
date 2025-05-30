using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CustodialHttpListener
{
    public static CustodialHttpListener Instance;
    private HttpListener _httpListener;
    private bool _tokenAuthListenerStarted = false;
    private const int ServerPort = 3000;
    private const string CallbackPath = "/callback";
    private const string Base64Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

    /// <summary>
    /// The expected state value for validating CSRF protection.
    /// </summary>
    public string ExpectedState { get; set; }
    
    public void StartTokenAuthListener(Action<string, string, string> onAuthCodeReceived)
    {
        if (_tokenAuthListenerStarted)
        {
            Debug.Log("Local web server is already started.");
            return;
        }

        string uri = $"http://localhost:{ServerPort}{CallbackPath}/";
        Debug.Log("Starting local web client at " + uri);

        _httpListener = new HttpListener();
        _httpListener.Prefixes.Add(uri);
        _httpListener.Start();
        _tokenAuthListenerStarted = true;

        // Start listening for requests asynchronously
        //Task.Run(() => ListenForTokenAuthRequests(onAuthCodeReceived));
        CoroutineSceneObject.Instance.StartCoroutine(ListenerRoutine(onAuthCodeReceived));
    }
    
    public void StopTokenAuthListener()
    {
        if (_tokenAuthListenerStarted)
        {
            CoroutineSceneObject.Instance.StopCoroutine(nameof(ListenerRoutine));
            _httpListener.Stop();
            _httpListener.Close();
            _tokenAuthListenerStarted = false;
            Debug.Log("Local web server stopped.");
        }
    }

    private IEnumerator ListenerRoutine(Action<string, string, string> onAuthCodeReceived)
    {
        while (_tokenAuthListenerStarted)
        {
            Task<HttpListenerContext> ctxTask = null;
            try
            {
                ctxTask = Task.Run(async () => await _httpListener.GetContextAsync());
            }
            catch (HttpListenerException ex)
            {
                Debug.LogError("HttpListener exception: " + ex.Message);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            yield return new WaitUntil(() => ctxTask.IsCompleted);
            try
            {
                var result = ctxTask.Result;
                ProcessListenerResponse(ctxTask.Result, onAuthCodeReceived);
            }
            catch
            {
                Debug.LogError("Failed to retrieve context");
            }
            
            yield return null;
        }
        yield break;
    }

    private void ProcessListenerResponse(HttpListenerContext context, Action<string,string,string> onCodeReceived)
    {
        var request = context.Request;
        
        if (request.HttpMethod == "GET" && request.Url.AbsolutePath is CallbackPath or CallbackPath + "/")
        {
            var queryParams = request.QueryString;
            string authCode = queryParams["code"];
            string state = queryParams["state"];

            if (string.IsNullOrEmpty(authCode))
            {
                Debug.LogError("Authorization code not found in the request.");
                return;
            }

            var response = context.Response;
            response.StatusCode = (int) HttpStatusCode.OK;
            response.ContentType = "text/plain";
            response.OutputStream.Write(System.Text.Encoding.UTF8.GetBytes("You may now close this webpage!"));
            response.OutputStream.Close();
            
            StopTokenAuthListener();
            onCodeReceived?.Invoke(authCode, state, ExpectedState);
        }
        else
        {
            onCodeReceived?.Invoke("", "", ExpectedState);
        }
    }
    
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
