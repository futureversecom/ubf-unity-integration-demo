// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Futureverse.UBF.Runtime.Settings;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class MakeFurMaterial : MakeMaterialBase
	{
		public MakeFurMaterial(Context context) : base(context) { }

		protected override void AddProperties(Dictionary<string, object> properties)
		{
			AddRenderMode(properties, "Render Mode", "_RenderMode");
			AddTexture(properties, "Diffuse Texture", "_DiffuseTex");
			AddColor(properties, "Base Color", "Tint");
			AddBool(properties, "Use Alpha", "_UseAlpha");
			AddFloat(properties, "Fresnel_IOR", "_Fresnel_IOR");
			AddFloat(properties, "Opacity", "_Opacity");

			AddBool(properties, "Use Emission", "_USEEMISSION");
			AddBool(properties, "Use Emissive Tint", "_UseEmissiveTint");
			AddTexture(properties, "Emissive Tex", "_EmissiveTex");
			AddFloat(properties, "Emissive Color Boost", "_EmissiveColorBoost");
			AddColor(properties, "Emissive Tint", "_EmissiveTint");
			AddFloat(properties, "Emissive Tint Boost", "_EmissiveTintBoost");

			AddBool(properties, "Use Normal Map", "_USENORMALMAP");
			AddTexture(properties, "Normal Tex", "_NormalTex");
			AddBool(properties, "Use ORM", "_USEORM");
			AddTexture(properties, "ORM", "_ORMTex");
			AddFloat(properties, "Occlusion", "_Occlusion");
			AddFloat(properties, "Roughness", "_Roughness");
			AddFloat(properties, "Metallic", "_Metallic");
			AddTexture(properties, "Height Map", "_HeightMap");
			AddTexture(properties, "Id Map", "_IdMap");
		}

		protected override Material GetMaterial => 
			TryRead<bool>("Use Alpha", out var property) && property ?
				UBFSettings.GetOrCreateSettings().FurTransparent :
				UBFSettings.GetOrCreateSettings().FurOpaque;
	}
}