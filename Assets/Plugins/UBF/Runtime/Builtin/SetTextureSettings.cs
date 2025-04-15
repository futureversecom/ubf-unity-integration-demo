// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class SetTextureSettings : ACustomNode
	{
		public SetTextureSettings(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<string>("Texture Resource", out var resourceId))
			{
				Debug.LogError("No valid resource given for SetTextureSettings");
				return;
			}

			var srgb = false;
			if (TryRead<bool>("sRGB", out var sRGB))
			{
				srgb = sRGB;
			}

			var importSettings = new TextureImportSettings();
			importSettings.UseSrgb = srgb;
			NodeContext.ExecutionContext.SetDynamicDataEntry(resourceId, Dynamic.Foreign(importSettings));

			WriteOutput("Texture", Dynamic.String(resourceId));
		}
	}

	public class TextureImportSettings
	{
		public bool UseSrgb;
	}
}