// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Futureverse.UBF.Runtime.Execution;

namespace Testbed.Local
{
	public class TestBlueprintInstanceData : IBlueprintInstanceData
	{
		public string ResourceId { get; }
		public string InstanceId { get; }
		public Dictionary<string, object> Inputs { get; } = new();
		
		public TestBlueprintInstanceData(string instanceId, string resourceId)
		{
			ResourceId = resourceId;
			InstanceId = instanceId;
		}

		public void AddInput(string key, object value)
		{
			Inputs.Add(key, value);
		}
	}
}