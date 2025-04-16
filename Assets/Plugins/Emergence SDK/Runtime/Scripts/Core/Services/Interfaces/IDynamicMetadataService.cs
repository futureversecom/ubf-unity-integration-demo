using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Types;
using EmergenceSDK.Runtime.Types.Delegates;

namespace EmergenceSDK.Runtime.Services
{
    /// <summary>
    /// Gives access to dynamic metadata.
    /// </summary>
    public interface IDynamicMetadataService : IEmergenceService
    {
        /// <summary>
        /// Attempts to write dynamic metadata to the specified contract, there must already be dynamic metadata on the object.
        /// </summary>
        /// <param name="network">Network of the contract we're reading the metadata from.</param>
        /// <param name="contract">The contact that we're reading from.</param>
        /// <param name="tokenId">The Token ID contract we're reading metadata from.</param>
        /// <param name="metadata">The metadata to write.</param>
        /// <param name="authorization">The authorization key of your app for the request</param>
        /// <param name="success">Delegate of type <see cref="SuccessWriteDynamicMetadata"/>, called in case of success</param>
        /// <param name="errorCallback">Delegate of type <see cref="ErrorCallback"/>, called in case of failure</param>
        /// <returns></returns>
        UniTask WriteDynamicMetadata(string network, string contract, string tokenId, string metadata, string authorization, SuccessWriteDynamicMetadata success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to write dynamic metadata to the specified contract, there must already be dynamic metadata on the object.
        /// </summary>
        /// <param name="network">Network of the contract we're reading the metadata from.</param>
        /// <param name="contract">The contact that we're reading from.</param>
        /// <param name="tokenId">The Token ID contract we're reading metadata from.</param>
        /// <param name="metadata">The metadata to write.</param>
        /// <param name="authorization">The authorization key of your app for the request</param>
        /// <returns>The transaction hash wrapped within a <see cref="ServiceResponse{T}"/></returns>
        UniTask<ServiceResponse<string>> WriteDynamicMetadataAsync(string network, string contract, string tokenId, string metadata, string authorization);

        /// <summary>
        /// Attempts to write dynamic metadata to the specified contract, there must not be dynamic metadata on the object.
        /// </summary>
        /// <param name="network">Network of the contract we're reading the metadata from.</param>
        /// <param name="contract">The contact that we're reading from.</param>
        /// <param name="tokenId">The Token ID contract we're reading metadata from.</param>
        /// <param name="metadata">The metadata to write.</param>
        /// <param name="authorization">The authorization key of your app for the request</param>
        /// <param name="success">Delegate of type <see cref="SuccessWriteDynamicMetadata"/>, called in case of success</param>
        /// <param name="errorCallback">Delegate of type <see cref="ErrorCallback"/>, called in case of failure</param>
        UniTask WriteNewDynamicMetadata(string network, string contract, string tokenId, string metadata, string authorization, SuccessWriteDynamicMetadata success, ErrorCallback errorCallback);

        /// <summary>
        /// Attempts to write dynamic metadata to the specified contract, there must not be dynamic metadata on the object.
        /// </summary>
        /// <param name="network">Network of the contract we're reading the metadata from.</param>
        /// <param name="contract">The contact that we're reading from.</param>
        /// <param name="tokenId">The Token ID contract we're reading metadata from.</param>
        /// <param name="metadata">The metadata to write.</param>
        /// <param name="authorization">The authorization key of your app for the request</param>
        /// <returns>The transaction hash wrapped within a <see cref="ServiceResponse{T}"/></returns>
        UniTask<ServiceResponse<string>> WriteNewDynamicMetadataAsync(string network, string contract, string tokenId, string metadata, string authorization);
    }
}