// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class SetTextureSettings : ACustomNode
	{
		public SetTextureSettings(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryReadResourceId("Texture Resource", out var resourceId) || !resourceId.IsValid)
			{
				return;
			}

			var srgb = false;
			if (TryRead<bool>("sRGB", out var sRGB))
			{
				srgb = sRGB;
			}

			var importSettings = new TextureImportSettings();
			importSettings.UseSrgb = srgb;
			NodeContext.ExecutionContext.SetDynamicDataEntry(resourceId.Value, Dynamic.Foreign(importSettings));

			WriteOutput("Texture", resourceId);
		}
	}

	public class TextureImportSettings
	{
		public bool UseSrgb;
	}
}