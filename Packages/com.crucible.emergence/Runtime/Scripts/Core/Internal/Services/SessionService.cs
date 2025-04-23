using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Internal.Types;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;
using EmergenceSDK.Runtime.Types.Responses;
using EmergenceSDK.Runtime.Utilities;
using UnityEngine;

namespace EmergenceSDK.Runtime.Internal.Services
{
    internal class SessionService : ISessionService, ISessionServiceInternal, ISessionConnectableService
    {
        public bool IsLoggedIn { get; private set; }
        public LoginSettings? CurrentLoginSettings { get; private set; }
        public event Action OnSessionConnected;
        public event Action OnSessionDisconnected;
        public string EmergenceAccessToken { get; private set; } = string.Empty;
        public bool DisconnectInProgress { get; private set; }

        public SessionService()
        {
            EmergenceSingleton.Instance.OnGameClosing += OnGameEnd;
        }
        
        public bool HasLoginSetting(LoginSettings loginSettings)
        {
            return CurrentLoginSettings?.HasFlag(loginSettings) ?? false;
        }

        private UniTask OnGameEnd() => DisconnectAsync();

        public async UniTask<ServiceResponse<IsConnectedResponse>> IsConnected()
        {
            try
            {
                var url = StaticConfig.APIBase + "isConnected";

                var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url, headers: EmergenceSingleton.DeviceIdHeader);
                if(!response.Successful)
                {
                    return new ServiceResponse<IsConnectedResponse>(false);
                }
                
                var successfulRequest = EmergenceUtils.ProcessResponse<IsConnectedResponse>(response, EmergenceLogger.LogError, out var processedResponse);
                if (successfulRequest)
                {
                    return new ServiceResponse<IsConnectedResponse>(true, processedResponse);
                }

                return new ServiceResponse<IsConnectedResponse>(false);
            }
            catch (Exception)
            {
                return new ServiceResponse<IsConnectedResponse>(false);
            }
        }

        public async UniTask<ServiceResponse> DisconnectAsync()
        {
            if (!IsLoggedIn || HasLoginSetting(LoginSettings.DisableEmergenceAccessToken) || string.IsNullOrEmpty(EmergenceAccessToken))
                return new ServiceResponse(true);
            
            try
            {
                DisconnectInProgress = true;

                var headers = EmergenceSingleton.DeviceIdHeader;
                headers.Add("auth", EmergenceAccessToken);
                var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, StaticConfig.APIBase + "killSession", headers: headers);
                if (!response.Successful)
                {
                    return new ServiceResponse(false);
                }

                if (EmergenceUtils.ResponseError(response))
                {
                    DisconnectInProgress = false;
                    return new ServiceResponse(false);
                }

                RunDisconnectionEvents();

                return new ServiceResponse(true);
            }
            catch (ArgumentException)
            {
                // Already disconnected
                return new ServiceResponse(true);
            }
            catch (Exception)
            {
                return new ServiceResponse(false);
            }
            finally
            {
                DisconnectInProgress = false;
            }
        }
        
        public void RunConnectionEvents(LoginSettings loginSettings)
        {
            IsLoggedIn = true;
            CurrentLoginSettings = loginSettings;
   
            foreach (var connectable in EmergenceServiceProvider.GetServices<ISessionConnectableService>())
            {
                connectable.HandleConnection(this);
            }
            OnSessionConnected?.Invoke();
        }

        public void RunDisconnectionEvents()
        {
            foreach (var connectable in EmergenceServiceProvider.GetServices<ISessionConnectableService>())
            {
                connectable.HandleDisconnection(this);
            }
            OnSessionDisconnected?.Invoke();

            IsLoggedIn = false;
            CurrentLoginSettings = null;
        }

        public async UniTask Disconnect(DisconnectSuccess success, ErrorCallback errorCallback)
        {
            var response = await DisconnectAsync();
            if(response.Successful)
                success?.Invoke();
            else
                errorCallback?.Invoke("Error in Disconnect.", (long)response.Code);
        }

        public async UniTask<ServiceResponse<Texture2D, string>> GetQrCodeAsync(CancellationToken ct)
        {
            try
            {
                var url = StaticConfig.APIBase + "qrcode";
                var response = await WebRequestService.DownloadTextureAsync(RequestMethod.Get, url, ct: ct);
                if (!response.Successful)
                {
                    return new ServiceResponse<Texture2D, string>(false);
                }
                
                if (EmergenceUtils.ResponseError(response))
                {
                    return new ServiceResponse<Texture2D, string>(false);
                }

                var deviceId = response.Headers["deviceId"];
                EmergenceSingleton.Instance.CurrentDeviceId = deviceId;
                return new ServiceResponse<Texture2D, string>(true, ((TextureWebResponse)response).Texture, response.Headers["walletconnecturi"]);
            }
            catch (Exception e) when (e is not OperationCanceledException) 
            {
                return new ServiceResponse<Texture2D, string>(false);
            }
        }

        public async UniTask GetQrCode(QRCodeSuccess success, ErrorCallback errorCallback, CancellationCallback cancellationCallback, CancellationToken ct)
        {
            try
            {
                var response = await GetQrCodeAsync(ct);
                if (response.Successful)
                    success?.Invoke(response.Result1);
                else
                    errorCallback?.Invoke("Error in GetQRCode.", (long)response.Code);
            }
            catch (OperationCanceledException)
            {
                cancellationCallback?.Invoke();
            }
        }
        
        public async UniTask<ServiceResponse<string>> GetAccessTokenAsync()
        {
            string url = StaticConfig.APIBase + "get-access-token";
            var headers = new Dictionary<string, string> { { "deviceId", EmergenceSingleton.Instance.CurrentDeviceId } };
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url, "", headers);
            if(!response.Successful)
                return new ServiceResponse<string>(false);
            var accessTokenResponse = SerializationHelper.Deserialize<BaseResponse<AccessTokenResponse>>(response.ResponseText);
            EmergenceAccessToken = SerializationHelper.Serialize(accessTokenResponse.message.AccessToken, false);
            return new ServiceResponse<string>(true, EmergenceAccessToken);
        }

        /// <summary>
        /// Method for caching an Emergence Access Token using an external custodial signing service.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async UniTask<ServiceResponse<string>> GetCustodialAccessToken(CancellationToken ct)
        {
            // Generate our timestamp used to create an EOT
            DateTime oneDayFromNow = DateTime.UtcNow.AddDays(1);
            long accessTokenTimestamp = new DateTimeOffset(oneDayFromNow).ToUnixTimeSeconds();
            string accessTokenMessage = $"{{\"expires-on\":{accessTokenTimestamp}}}";
            
            var custodialSigningService = EmergenceServiceProvider.GetService<ICustodialSigningService>();
            var custodialLoginService = EmergenceServiceProvider.GetService<ICustodialLoginService>();

            string eoa = custodialLoginService.CachedAccessTokenResponse.DecodedToken.Eoa; // Get currently cached EOA/wallet address
            // Use the external signing service to perform a signature request and return an emergence access token, caching it as part of the session.
            string eatResponse =  await custodialSigningService.GenerateEAT(eoa, accessTokenMessage, accessTokenTimestamp.ToString());
            if (!string.IsNullOrEmpty(eatResponse))
            {
                EmergenceAccessToken = eatResponse;
                EmergenceSingleton.Instance.IsCustodialLogin = true;
                return new ServiceResponse<string>(true, EmergenceAccessToken);
            }
            else
            {
                await UniTask.SwitchToMainThread();
                return new ServiceResponse<string>(false, "Custodial Signing Service Failure");
            }

        }
        

        public void HandleDisconnection(ISessionService sessionService)
        {
            EmergenceAccessToken = "";
            if (EmergenceSingleton.Instance.IsCustodialLogin)
            {
                EmergenceSingleton.Instance.IsCustodialLogin = false;
            }
        }
        
        public void HandleConnection(ISessionService sessionService) { }

        public async UniTask GetAccessToken(AccessTokenSuccess success, ErrorCallback errorCallback)
        {
            var response = await GetAccessTokenAsync();
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in GetAccessToken.", (long)response.Code);
        }
    }
}
