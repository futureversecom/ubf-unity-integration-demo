// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using Futureverse.UBF.Runtime;
using Futureverse.UBF.Runtime.Resources;
using Futureverse.UBF.Runtime.Utils;
using Newtonsoft.Json.Linq;

namespace Plugins.UBF.Runtime.Builtin.ResourceLoading
{
	public class CreateMeshResource : ACustomExecNode
	{
		public CreateMeshResource(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead("URI", out string uri))
			{
				UbfLogger.LogError("[CreateMeshResource] Could not find input \"URI\"");
				return;
			}
			
			if (!TryRead("MeshID", out string meshId))
			{
				UbfLogger.LogError("[CreateMeshResource] Could not find input \"Mesh ID\"");
				return;
			}

			var settings = new MeshAssetImportSettings(NodeContext.ExecutionContext.BlueprintVersion, meshId);
			var resourceId = Guid.NewGuid().ToString();
			var resource = new ResourceData(
				resourceId,
				uri,
				ResourceType.Mesh,
				JObject.FromObject(settings)
			);
			
			NodeContext.ExecutionContext.Config.RegisterRuntimeResource(resource);
			WriteOutput("Resource", resourceId);
		}
	}
}