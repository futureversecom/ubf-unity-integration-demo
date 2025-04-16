// Placeholder Copyright Header

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Futureverse.Services;
using EmergenceSDK.Runtime.Services;
using UnityEngine;
using UnityEngine.Events;

namespace Futureverse.FuturePass
{
	[RequireComponent(typeof(FuturePassLoginManager))]
	public class FuturePassInventoryManager : MonoBehaviour
	{
		public List<string> collectionIds = new();

		public bool preloadImages;

		public UnityEvent<List<FuturePassInventoryItem>> InventoryReady = new();
		private IFutureverseInventoryService futureverseInventoryService;

		private FuturePassLoginManager loginManager;

		public List<FuturePassInventoryItem> Content { get; } = new();

		private void Awake()
		{
			loginManager = GetComponent<FuturePassLoginManager>();
		}

		private void OnEnable()
		{
			loginManager.Connected.AddListener(Initiailize);
		}

		private void OnDisable()
		{
			loginManager.Connected.RemoveListener(Initiailize);
		}

		public void Initiailize()
		{
			futureverseInventoryService = EmergenceServiceProvider.GetService<IFutureverseInventoryService>();

			if (futureverseInventoryService == null)
			{
				Debug.LogError("Inventory service is null!");

				return;
			}

			Initialize()
				.Forget();
		}

		public async UniTaskVoid Initialize()
		{
			var inventoryResponse = await futureverseInventoryService.InventoryByOwnerAndCollectionAsync(collectionIds);

			if (!inventoryResponse.Successful)
			{
				Debug.LogError("Failed to get futurepass inventory.");

				return;
			}

			Debug.Log($"Retrieved linked Futureverse inventory containing {inventoryResponse.Result1.Count} items.");

			var tasks = new List<UniTask>();

			foreach (var inventoryItem in inventoryResponse.Result1)
			{
				var content = new FuturePassInventoryItem(inventoryItem);

				if (preloadImages)
				{
					tasks.Add(content.GetImage());
				}

				Content.Add(content);
			}

			try
			{
				await UniTask.WhenAll(tasks);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}

			InventoryReady.Invoke(Content);
		}
	}
}