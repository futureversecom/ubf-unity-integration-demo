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
			AddTexture(properties, "Base Color Tex", "_BaseColorTex");
			AddFloat(properties, "Ambient Occlusion", "_AmbientOcclusion");
			AddColor(properties, "Color Tint", "_ColorTint");
			AddTexture(properties, "ORM Tex", "_ORMTex");
			AddFloat(properties, "Metallic", "_Metallic");
			AddFloat(properties, "Roughness", "_Roughness");
			
			AddTexture(properties, "Normal Tex", "_NormalTex");
			AddFloat(properties, "Normal Strength", "_NormalStrength");
			AddBool(properties, "Flip Normal", "_NormalFlip");
			
			AddBool(properties, "Fresnel", "_FRESNEL");
			AddColor(properties, "Fresnel Color", "_FresnelColor");
			AddFloat(properties, "Fresnel Power", "_FresnelPower");
			
			AddBool(properties, "Use Emission", "_EMISSIVE");
			AddColor(properties, "Emissive Tint", "_EmissiveTint");
			AddFloat(properties, "Emissive Strength", "_EmissiveStrength");
			AddTexture(properties, "Emissive Tex", "_EmissiveTex");
			
			AddTexture(properties, "Freckle Tex", "_FreckleTex");
			AddFloat(properties, "Freckle Opacity", "_FreckleOpacity");
			AddColor(properties, "Freckle Tint", "_FreckleTint");
			
			AddTexture(properties, "Tattoo Tex", "_TattooTex");
			AddColor(properties, "Tattoo Tint", "_TattooTint");
			
			AddTexture(properties, "Beard Tex", "_BeardTex");
			AddFloat(properties, "Beard Opacity", "_BeardOpacity");
			AddColor(properties, "Beard Tint", "_BeardTint");
			
			AddTexture(properties, "Face Paint Tex", "_FacePaintTex");
			AddFloat(properties, "Face Paint Opacity", "_FacePaintOpacity");
		}

		protected override Material GetMaterial
			=> UBFSettings.GetOrCreateSettings()
				.Skin02;
	}
}