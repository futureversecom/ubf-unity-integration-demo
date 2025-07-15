// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using Futureverse.UBF.Runtime.Resources;
using Futureverse.UBF.Runtime.Utils;
using Newtonsoft.Json.Linq;

namespace Futureverse.UBF.Runtime.Builtin.ResourceLoading
{
	public class CreateBlueprintResource : ACustomExecNode
	{
		public CreateBlueprintResource(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead("URI", out string uri))
			{
				UbfLogger.LogError("[CreateBlueprintResource] Could not find input \"URI\"");
				return;
			}

			var settings = new BlueprintAssetImportSettings(NodeContext.ExecutionContext.BlueprintVersion);
			var resourceId = Guid.NewGuid().ToString();
			var resource = new ResourceData(
				resourceId,
				uri,
				ResourceType.Blueprint,
				JObject.FromObject(settings)
			);
			
			NodeContext.ExecutionContext.Config.RegisterRuntimeResource(resource);
			WriteOutput("Resource", resourceId);
		}
	}
}