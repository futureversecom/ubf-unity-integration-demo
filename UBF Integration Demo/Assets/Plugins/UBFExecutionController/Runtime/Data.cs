// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Resources;
using Newtonsoft.Json.Linq;

namespace Futureverse.UBF.ExecutionController.Runtime
{
	public interface IUbfAsset : IUbfData
	{
		public string CollectionId { get; }
		public JObject Metadata { get; }
		public string TokenId { get; }
	}

	public interface IUbfDataParser
	{
		IEnumerator GetBlueprintDefinition(IUbfData data, Action<IBlueprintInstanceData, Catalog> callback);
	}

	public interface IUbfData
	{
		string Id { get; }
	}

	public interface IUbfTree
	{
		public interface IUbfTreeNode
		{
			IUbfData NodeData { get; }
			Dictionary<string, IUbfData> Children { get; }
		}

		IUbfData RootData { get; }
		IUbfTreeNode[] TreeNodes { get; }
	}
}