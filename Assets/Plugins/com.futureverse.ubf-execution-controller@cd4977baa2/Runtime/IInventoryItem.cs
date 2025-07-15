// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Futureverse.UBF.UBFExecutionController.Runtime
{
	public interface IInventoryItem
	{
		string Id { get; }
		string Name { get; }
		string CollectionId { get; }
		string TokenId { get; }
		AssetProfile AssetProfile { get; }
		JObject Metadata { get; }
		Dictionary<string, IInventoryItem> Children { get; }
	}
}