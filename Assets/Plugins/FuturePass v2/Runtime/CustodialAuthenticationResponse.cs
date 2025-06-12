using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public class CustodialAuthenticationResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
            
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
