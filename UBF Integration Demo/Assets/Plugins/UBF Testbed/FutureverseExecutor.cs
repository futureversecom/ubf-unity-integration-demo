// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections;
using Futureverse.UBF.ExecutionController.Runtime;
using UnityEngine;

namespace Testbed
{
	public class FutureverseExecutor : MonoBehaviour
	{
		[SerializeField] private string _assetTreeUri;
		[SerializeField] private FutureverseRuntimeController _controller;

		[ContextMenu("Execute")]
		public void Execute()
		{
			StartCoroutine(ExecuteRoutine());
		}

		private IEnumerator ExecuteRoutine()
		{
			yield break;
			// AssetTreeData assetTree = null;
			// var assetTreeLoader = new ResourceLoader<AssetTreeData>(
			// 	new BasicResource(_assetTreeUri),
			// 	new DefaultDownloader(),
			// 	new JsonLoader<AssetTreeData>()
			// );
			//
			// yield return assetTreeLoader.Get((_, tree) => assetTree = tree);
			// yield return _controller.ExecuteAssetTree(assetTree, null);
		}
	}
}