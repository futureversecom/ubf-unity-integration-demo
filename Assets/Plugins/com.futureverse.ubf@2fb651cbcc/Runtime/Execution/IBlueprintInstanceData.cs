// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Utils;

namespace Futureverse.UBF.Runtime.Execution
{
	/// <summary>
	/// Contains the necessary info to load a Blueprint from a Catalog at runtime.
	/// </summary>
	public interface IBlueprintInstanceData
	{
		/// <summary>
		/// The Resource ID that will be used to retrieve the resource from a Catalog.
		/// </summary>
		string ResourceId { get; }
		/// <summary>
		/// A Globally unique identifier that is used to index the loaded Blueprint.
		/// </summary>
		string InstanceId { get; }
		/// <summary>
		/// User inputs that are passed into the loaded Blueprint.
		/// </summary>
		Dictionary<string, object> Inputs { get; }
	}
	
	public class BlueprintInstanceData : IBlueprintInstanceData
	{
		public string ResourceId { get; }
		public string InstanceId { get; }
		public Dictionary<string, object> Inputs { get; set; } = new();
		
		/// <summary>
		/// Assigns a Resource ID and a new GUID as the Instance ID.
		/// </summary>
		/// <param name="resourceId">The Resource ID that will be used to retrieve the resource from a Catalog.</param>
		public BlueprintInstanceData(string resourceId)
		{
			ResourceId = resourceId;
			InstanceId = Guid.NewGuid()
				.ToString();
		}
		
		public BlueprintInstanceData(string resourceId, string instanceId)
		{
			ResourceId = resourceId;
			InstanceId = instanceId;
		}

		/// <summary>
		/// Adds a new input that will be assigned to the Blueprint.
		/// </summary>
		/// <param name="key">The input name.</param>
		/// <param name="value">The input value.</param>
		public void AddInput(string key, object value)
		{
			if (!Inputs.TryAdd(key, value))
			{
				UbfLogger.LogError($"Attempted to add duplicate input key {key}");
			}
		}
	}
}