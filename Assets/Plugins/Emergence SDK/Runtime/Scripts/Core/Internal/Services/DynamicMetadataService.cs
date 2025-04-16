using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;

namespace EmergenceSDK.Runtime.Internal.Services
{
    internal class DynamicMetadataService : IDynamicMetadataService
    {
        public async UniTask<ServiceResponse<string>> WriteNewDynamicMetadataAsync(string network, string contract, string tokenId, string metadata, string authorization)
        {
            string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "putMetadata?network=" + network + "&contract=" + contract + "&tokenId=" + tokenId;

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization-header", authorization);
            var bodyData = "{\"metadata\": \"" + metadata + "\"}";
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Put, url, bodyData, headers);
            if(!response.Successful)
                return new ServiceResponse<string>(false);
            
            return new ServiceResponse<string>(true, response.ResponseText);
        }

        public async UniTask WriteNewDynamicMetadata(string network, string contract, string tokenId, string metadata, string authorization,
            SuccessWriteDynamicMetadata success, ErrorCallback errorCallback)
        {
            var response = await WriteNewDynamicMetadataAsync(network, contract, tokenId, metadata, authorization);
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in WriteDynamicMetadata.", (long)response.Code);
        }
        
        public async UniTask<ServiceResponse<string>> WriteDynamicMetadataAsync(string network, string contract, string tokenId, string metadata, string authorization)
        {
            string url = EmergenceSingleton.Instance.Configuration.InventoryURL + "updateMetadata?network=" + network + "&contract=" + contract + "&tokenId=" + tokenId;

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization-header", authorization);
            var bodyData = "{\"metadata\": \"" + metadata + "\"}";
            var response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Post, url, bodyData, headers);
            if(!response.Successful)
                return new ServiceResponse<string>(false);
            
            return new ServiceResponse<string>(true, response.ResponseText);
        }

        public async UniTask WriteDynamicMetadata(string network, string contract, string tokenId, string metadata, string authorization,
            SuccessWriteDynamicMetadata success, ErrorCallback errorCallback)
        {
            var response = await WriteDynamicMetadataAsync(network, contract, tokenId, metadata, authorization);
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in WriteDynamicMetadata.", (long)response.Code);
        }
        
        public async UniTask<ServiceResponse<string>> WriteNewDynamicMetadataAsync(string network, string contract, string tokenId, string metadata)
        {
            return await WriteNewDynamicMetadataAsync(network, contract, tokenId, metadata, "0iKoO1V2ZG98fPETreioOyEireDTYwby");
        }

        public async UniTask WriteNewDynamicMetadata(string network, string contract, string tokenId, string metadata,
            SuccessWriteDynamicMetadata success, ErrorCallback errorCallback)
        {
            await WriteNewDynamicMetadata(network, contract, tokenId, metadata, "0iKoO1V2ZG98fPETreioOyEireDTYwby", success, errorCallback);
        }
        
        public async UniTask<ServiceResponse<string>> WriteDynamicMetadataAsync(string network, string contract, string tokenId, string metadata)
        {
            return await WriteDynamicMetadataAsync(network, contract, tokenId, metadata, "0iKoO1V2ZG98fPETreioOyEireDTYwby");
        }

        public async UniTask WriteDynamicMetadata(string network, string contract, string tokenId, string metadata,
            SuccessWriteDynamicMetadata success, ErrorCallback errorCallback)
        {
            await WriteDynamicMetadata(network, contract, tokenId, metadata, "0iKoO1V2ZG98fPETreioOyEireDTYwby", success, errorCallback);
        }
    }
}