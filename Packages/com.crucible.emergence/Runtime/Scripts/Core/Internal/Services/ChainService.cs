using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Internal.Types;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;
using EmergenceSDK.Runtime.Types.Responses;

namespace EmergenceSDK.Runtime.Internal.Services
{
    public class ChainService : IChainService
    {
        public async UniTask<ServiceResponse<GetTransactionStatusResponse>> GetTransactionStatusAsync(string transactionHash, string nodeURL)
        {
            string url = StaticConfig.APIBase + "GetTransactionStatus?transactionHash=" + transactionHash + "&nodeURL=" + nodeURL;
            WebResponse response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url);
            if(!response.Successful)
                return new ServiceResponse<GetTransactionStatusResponse>(false);
            var transactionStatusResponse = SerializationHelper.Deserialize<BaseResponse<GetTransactionStatusResponse>>(response.ResponseText);
            return new ServiceResponse<GetTransactionStatusResponse>(true, transactionStatusResponse.message);
        }

        public async UniTask GetTransactionStatus(string transactionHash, string nodeURL, GetTransactionStatusSuccess success, ErrorCallback errorCallback)
        {
            var response = await GetTransactionStatusAsync(transactionHash, nodeURL);
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in GetTransactionStatus.", (long)response.Code);
        }

        public async UniTask<ServiceResponse<GetBlockNumberResponse>> GetHighestBlockNumberAsync(string nodeURL)
        {
            string url = StaticConfig.APIBase + "getBlockNumber?nodeURL=" + nodeURL;
            WebResponse response = await WebRequestService.SendAsyncWebRequest(RequestMethod.Get, url);
            if(!response.Successful)
                return new ServiceResponse<GetBlockNumberResponse>(false);
            var blockNumberResponse = SerializationHelper.Deserialize<BaseResponse<GetBlockNumberResponse>>(response.ResponseText);
            return new ServiceResponse<GetBlockNumberResponse>(true, blockNumberResponse.message);
        }

        public async UniTask GetHighestBlockNumber(string nodeURL, GetBlockNumberSuccess success, ErrorCallback errorCallback)
        {
            var response = await GetHighestBlockNumberAsync(nodeURL);
            if(response.Successful)
                success?.Invoke(response.Result1);
            else
                errorCallback?.Invoke("Error in GetHighestBlockNumber.", (long)response.Code);
        }
    }
}