// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Testbed.AssetRegister
{
	public class AssetUI : MonoBehaviour
	{
		private static readonly Dictionary<string, Sprite> s_spriteCache = new();
		
		[SerializeField] public Image _image;
		[SerializeField] public Button _button;
		[SerializeField] public Text _tokenIdText;
		
		public void Load(InventoryNode asset, Action onClick)
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(() => onClick?.Invoke());

			_tokenIdText.text = asset.TokenId;
			var imageUri = asset.Metadata["metadata"]?["properties"]?["image"]
				?.ToString();
			if (imageUri != null)
			{
				StartCoroutine(LoadBackgroundImage(asset.TokenId, imageUri));
			}
		}

		private IEnumerator LoadBackgroundImage(string tokenId, string imageUri)
		{
			if (s_spriteCache.TryGetValue(tokenId, out var sprite))
			{
				_image.sprite = sprite;
				yield break;
			}
			
			var webRequest = new UnityWebRequest(imageUri, "GET");
			webRequest.downloadHandler = new DownloadHandlerTexture();
			yield return webRequest.SendWebRequest();

			if (webRequest.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Couldn't load image at {imageUri} - {webRequest.error}");
				yield break;
			}
			
			var texture = (webRequest.downloadHandler as DownloadHandlerTexture)?.texture;
			if (texture == null)
			{
				Debug.LogError($"Image at {imageUri} is null");
				yield break;
			}

			sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
			s_spriteCache.TryAdd(tokenId, sprite);
			_image.sprite = sprite;
		}

		private void OnDestroy()
		{
			StopAllCoroutines();
		}
	}
}