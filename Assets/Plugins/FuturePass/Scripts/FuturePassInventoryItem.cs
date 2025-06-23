// Placeholder Copyright Header

using System.IO;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Types.Inventory;
using Futureverse.UBF.ExecutionController.Runtime;
using Futureverse.UBF.Runtime.Utils;
using GLTFast.Schema;
using UnityEngine;
using UnityEngine.Networking;

namespace Futureverse.FuturePass
{
	public class FuturePassInventoryItem
	{
		public FuturePassInventoryItem() { }

		public FuturePassInventoryItem(InventoryItem inventoryItem)
		{
			InventoryItem = inventoryItem;
			//UbfAssetProfile = UbfController.Config.GetProfileForInventoryItem(inventoryItem);

			// CoroutineHost.Instance.StartCoroutine(AssetProfile.FetchByAssetId(inventoryItem.ID.Replace(":"+inventoryItem.TokenId,""), inventoryItem.Meta.Name, (profile) => UbfAssetProfile = profile));
		}

		public InventoryItem InventoryItem { get; }
		public AssetProfile UbfAssetProfile { get; private set; }
		public Texture2D Image { get; private set; }

        /// <summary>
        ///     Downloads a Texture2D from the url defined in Properties.image or loads it from disk if it is cached.
        ///     The texture is stored in the Image property and will be returned immediately if it has already been loaded.
        /// </summary>
        /// <returns>
        ///     The loaded Texture2D stored in the Image property
        /// </returns>
        public async UniTask<Texture2D> GetImage(bool forceDownload = false)
		{
			var cacheDirectory = $"{Application.persistentDataPath}/FutureverseContent/Images";

			if (!Directory.Exists(cacheDirectory))
			{
				Directory.CreateDirectory(cacheDirectory);
			}

			if (!forceDownload && Image is not null)
			{
				return Image;
			}

			var url = string.Empty;

			//default to optional png image property
			try
			{
				url = InventoryItem.OriginalData?["metadata"]?["properties"]?["image_png"]
					?.ToObject<string>();
			}
			catch
			{
				Debug.Log("original data blew up");
			}

			if (string.IsNullOrEmpty(url))
			{
				url = InventoryItem.Meta.Content?[0].URL;
			}

			var fileName = url.GetHashCode()
				.ToString();
			var filePath = $"{cacheDirectory}/{fileName}";
			var cacheResult = true;

			if (!forceDownload && File.Exists(filePath))
			{
				url = $"file://{filePath}";

				cacheResult = false;
			}

			//use public gateway for ipfs, see https://docs.ipfs.tech/how-to/address-ipfs-on-web/#ipfs-addressing-in-brief
			else if (url.Contains("ipfs://"))
			{
				var cid = url.Substring(url.LastIndexOf('/') + 1);

				url = $"https://ipfs.io/ipfs/{cid}";
			}

			using var request = UnityWebRequestTexture.GetTexture(url);

			//weirdly we need this try/catch to catch download handler errors
			try
			{
				await request.SendWebRequest();

				if (request.result != UnityWebRequest.Result.Success)
				{
					LogError($"{request.result}, {request.downloadHandler.text}");

					return null;
				}

				Image = (request.downloadHandler as DownloadHandlerTexture).texture;
			}
			catch
			{
				Debug.LogWarning(
					$"UnityWebRequestTexture.GetTexture failed. Falling back on manual creation.\n{request.result}, {request.downloadHandler.error}"
				);

				try
				{
					var texture = new Texture2D(1, 1);

					texture.LoadImage(request.downloadHandler.data);

					Image = texture;
				}
				catch
				{
					LogError($"Failed to get image for {InventoryItem.TokenId}");

					return null;
				}
			}

			if (cacheResult)
			{
				File.WriteAllBytes(filePath, request.downloadHandler.data);
			}

			return Image;

			void LogError(string error)
			{
				Debug.LogError($"Error getting image for node id {InventoryItem.TokenId}: {error}");
			}
		}
	}
}