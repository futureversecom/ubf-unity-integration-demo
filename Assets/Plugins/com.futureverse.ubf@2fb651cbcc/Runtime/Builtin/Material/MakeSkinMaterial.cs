// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Futureverse.UBF.Runtime.Settings;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class MakeSkinMaterial : MakeMaterialBase
	{
		public MakeSkinMaterial(Context context) : base(context) { }

		protected override void AddProperties(Dictionary<string, object> properties)
		{
			AddTexture(properties, "GCLSTexture", "_GCLSTex");
			AddTexture(properties, "NormalTexture", "_NormalTex");
			AddTexture(properties, "ORSTexture", "_ORSTex");

			AddColor(properties, "SkinColor", "_SkinColor");
			AddColor(properties, "Redness", "_RednessColor");
			AddColor(properties, "LipColor", "_LipsColor");

			AddFloat(properties, "SkinVariation", "_SkinVariation");
			AddFloat(properties, "DarkAreaHue", "_SkinColorDark_Hue");
			AddFloat(properties, "DarkAreaSaturation", "_SkinColorDark_Saturate");
			AddFloat(properties, "DarkAreaValue", "_SkinColorDark_Value");
			AddFloat(properties, "RoughnessStrength", "_RoughnessStrength");
			AddFloat(properties, "AOStrength", "_AOStrength");

			AddTexture(properties, "BuzzcutTexture", "_BuzzcutTex");
			AddTexture(properties, "StubbleTexture", "_StubbleTex");
			AddTexture(properties, "FrecklesTexture", "_FrecklesTex");
			AddTexture(properties, "MolesTexture", "_MolesTex");
			AddTexture(properties, "ScarsTexture", "_ScarsTex");

			AddColor(properties, "HairColor", "_HairColor");
			AddColor(properties, "FreckleColor", "_FreckleColor");
			AddColor(properties, "MoleColor", "_MoleColor");
			AddColor(properties, "ScarColor", "_ScarColor");

			AddFloat(properties, "StubbleGrowth", "_StubbleGrowth");
			AddFloat(properties, "StubbleContrast", "_StubbleContrast");
			AddFloat(properties, "MoleNormalStrength", "_MoleNormalStrength");
			AddFloat(properties, "ScarColorContrast", "_ScarColorContrast");
			AddFloat(properties, "ScarColorFalloff", "_ScarColorFalloff");
			AddFloat(properties, "ScarNormalStrength", "_ScarNormalStrength");
		}

		protected override Material GetMaterial
			=> UBFSettings.GetOrCreateSettings()
				.Skin;
	}
}