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
			AddTexture(properties, "GCLS Tex", "_GCLSTex");
			AddTexture(properties, "Normal Tex", "_NormalTex");
			AddTexture(properties, "ORS Tex", "_ORSTex");

			AddColor(properties, "Skin Color", "_SkinColor");
			AddColor(properties, "Redness", "_RednessColor");
			AddColor(properties, "Lip Color", "_LipsColor");

			AddFloat(properties, "Skin Variation", "_SkinVariation");
			AddFloat(properties, "Dark Area Hue", "_SkinColorDark_Hue");
			AddFloat(properties, "Dark Area Saturation", "_SkinColorDark_Saturate");
			AddFloat(properties, "Dark Area Value", "_SkinColorDark_Value");
			AddFloat(properties, "Roughness Strength", "_RoughnessStrength");
			AddFloat(properties, "AO Strength", "_AOStrength");

			AddTexture(properties, "Buzzcut Tex", "_BuzzcutTex");
			AddTexture(properties, "Stubble Tex", "_StubbleTex");
			AddTexture(properties, "Freckles Tex", "_FrecklesTex");
			AddTexture(properties, "Moles Tex", "_MolesTex");
			AddTexture(properties, "Scars Tex", "_ScarsTex");

			AddColor(properties, "Hair Color", "_HairColor");
			AddColor(properties, "Freckle Color", "_FreckleColor");
			AddColor(properties, "Mole Color", "_MoleColor");
			AddColor(properties, "Scar Color", "_ScarColor");

			AddFloat(properties, "Stubble Growth", "_StubbleGrowth");
			AddFloat(properties, "Stubble Contrast", "_StubbleContrast");
			AddFloat(properties, "Mole Normal Strength", "_MoleNormalStrength");
			AddFloat(properties, "Scar Color Contrast", "_ScarColorContrast");
			AddFloat(properties, "Scar Color Falloff", "_ScarColorFalloff");
			AddFloat(properties, "Scar Normal Strength", "_ScarNormalStrength");
		}

		protected override Material GetMaterial
			=> UBFSettings.GetOrCreateSettings()
				.Skin;
	}
}