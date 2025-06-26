// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using AssetRegister.Runtime.Schema.Objects;
using Futureverse.UBF.UBFExecutionController.Runtime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Futureverse.UBF.UBFExecutionController.Samples
{
	public class AssetUI : MonoBehaviour
	{
		private static readonly Dictionary<string, Sprite> s_spriteCache = new();
		
		[SerializeField] public Image _image;
		[SerializeField] public Button _button;
		[SerializeField] public Text _tokenIdText;
		
		public void Load(Asset asset, Action onClick)
		{
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(() => onClick?.Invoke());

			_tokenIdText.text = asset.TokenId;
			var imageUri = asset.GetProfileImageUrl();
			if (imageUri != null)
			{
				StartCoroutine(LoadBackgroundImage(asset.TokenId, imageUri));
			}
		}

		private IEnumerator LoadBackgroundImage(string tokenId, string imageUri)
		{
			// Webp format not supported by unity. don't try to load the image if that's the case.
			if (imageUri.EndsWith(".webp") || imageUri.EndsWith(".gif") || imageUri.StartsWith("ipfs:"))
			{
				yield break;
			}
			
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