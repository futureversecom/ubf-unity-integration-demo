// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using Futureverse.UBF.Runtime.Resources;
using Futureverse.UBF.Runtime.Utils;
using Newtonsoft.Json.Linq;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class CreateTextureResource : ACustomExecNode
	{
		public CreateTextureResource(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead("URI", out string uri))
			{
				UbfLogger.LogError("[CreateTextureResource] Could not find input \"URI\"");
				return;
			}
			
			// Currently unused, but will be added to TextureAssetImportSettings in a future update.
			if (!TryRead("SRGB", out bool srgb))
			{
				UbfLogger.LogError("[CreateTextureResource] Could not find input \"Use SRGB\"");
				return;
			}

			var settings = new TextureAssetImportSettings(NodeContext.ExecutionContext.BlueprintVersion, srgb);
			var resourceId = Guid.NewGuid().ToString();
			var resource = new ResourceData(
				resourceId,
				uri,
				ResourceType.Texture,
				JObject.FromObject(settings)
			);
			
			NodeContext.ExecutionContext.Config.RegisterRuntimeResource(resource);
			WriteOutput("Resource", resourceId);
		}
	}
}