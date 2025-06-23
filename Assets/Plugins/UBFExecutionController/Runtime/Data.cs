// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Resources;
using Newtonsoft.Json.Linq;

namespace Futureverse.UBF.ExecutionController.Runtime
{
	/// <summary>
	/// Contains required information about a given NFT Asset
	/// </summary>
	public interface IUbfAsset : IUbfData
	{
		/// <summary>
		/// ID of the chain the NFT exists on
		/// </summary>
		string ChainId { get; }
		/// <summary>
		/// Chain Name or Type
		/// </summary>
		string ChainName { get; }
		/// <summary>
		/// ID of the NFT Collection.
		/// </summary>
		string CollectionId { get; }
		/// <summary>
		/// Metadata of the NFT in Json format.
		/// </summary>
		JObject Metadata { get; }
		/// <summary>
		/// Token ID of the specific NFT asset.
		/// </summary>
		string TokenId { get; }
	}

	/// <summary>
	/// Describes how UbfData is turned into a set of BlueprintInstanceData and Catalog.
	/// </summary>
	public interface IUbfDataParser
	{
		/// <summary>
		/// Turns UbfData into specific Blueprint and Catalog data required to run.
		/// </summary>
		/// <param name="data">The base UbfData.</param>
		/// <param name="callback">Callback containing the BlueprintInstanceData and Catalog.</param>
		/// <returns></returns>
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