// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Futureverse.UBF.Runtime.Settings;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class MakeSkin02Material : MakeMaterialBase
	{
		public MakeSkin02Material(Context context) : base(context) { }

		protected override void AddProperties(Dictionary<string, object> properties)
		{
			AddTexture(properties, "BaseColorTexture", "_BaseColorTex");
			AddFloat(properties, "AmbientOcclusion", "_AmbientOcclusion");
			AddColor(properties, "ColorTint", "_ColorTint");
			AddTexture(properties, "ORMTexture", "_ORMTex");
			AddFloat(properties, "Metallic", "_Metallic");
			AddFloat(properties, "Roughness", "_Roughness");
			
			AddTexture(properties, "NormalTexture", "_NormalTex");
			AddFloat(properties, "NormalStrength", "_NormalStrength");
			AddBool(properties, "FlipNormal", "_NormalFlip");
			
			AddBool(properties, "Fresnel", "_FRESNEL");
			AddColor(properties, "FresnelColor", "_FresnelColor");
			AddFloat(properties, "FresnelPower", "_FresnelPower");
			
			AddBool(properties, "UseEmission", "_EMISSIVE");
			AddColor(properties, "EmissiveTint", "_EmissiveTint");
			AddFloat(properties, "EmissiveStrength", "_EmissiveStrength");
			AddTexture(properties, "EmissiveTexture", "_EmissiveTex");
			
			AddTexture(properties, "FreckleTexture", "_FreckleTex");
			AddFloat(properties, "FreckleOpacity", "_FreckleOpacity");
			AddColor(properties, "FreckleTint", "_FreckleTint");
			
			AddTexture(properties, "TattooTexture", "_TattooTex");
			AddColor(properties, "TattooTint", "_TattooTint");
			
			AddTexture(properties, "BeardTexture", "_BeardTex");
			AddFloat(properties, "BeardOpacity", "_BeardOpacity");
			AddColor(properties, "BeardTint", "_BeardTint");
			
			AddTexture(properties, "FacePaintTexture", "_FacePaintTex");
			AddFloat(properties, "FacePaintOpacity", "_FacePaintOpacity");
		}

		protected override Material GetMaterial
			=> UBFSettings.GetOrCreateSettings()
				.Skin02;
	}
}