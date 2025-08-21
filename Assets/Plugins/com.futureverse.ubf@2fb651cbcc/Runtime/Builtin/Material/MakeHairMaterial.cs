// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Futureverse.UBF.Runtime.Settings;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class MakeHairMaterial : MakeMaterialBase
	{
		public MakeHairMaterial(Context context) : base(context) { }

		protected override void AddProperties(Dictionary<string, object> properties)
		{
			AddRenderMode(properties, "RenderMode", "_RenderMode");
			AddTexture(properties, "DiffuseTexture", "_DiffuseTex");
			AddColor(properties, "BaseColor", "_Tint");
			AddBool(properties, "UseAlpha", "_UseAlpha");
			AddFloat(properties, "FresnelIOR", "_Fresnel_IOR");
			AddFloat(properties, "Opacity", "_Opacity");
			AddTexture(properties, "AlphaTexture", "_AlphaTex");
			AddBool(properties, "UseAlphaTexture", "_UseAlphaTex");

			AddBool(properties, "UseEmission", "_USEEMISSION");
			AddBool(properties, "UseEmissiveTint", "_UseEmissiveTint");
			AddTexture(properties, "EmissiveTexture", "_EmissiveTex");
			AddFloat(properties, "EmissiveColorBoost", "_EmissiveColorBoost");
			AddColor(properties, "EmissiveTint", "_EmissiveTint");
			AddFloat(properties, "EmissiveTintBoost", "_EmissiveTintBoost");

			AddBool(properties, "UseNormalMap", "_USENORMALMAP");
			AddTexture(properties, "NormalTexture", "_NormalTex");
			AddBool(properties, "UseORM", "_USEORM");
			AddTexture(properties, "ORM", "_ORMTex");
			AddFloat(properties, "Occlusion", "_Occlusion");
			AddFloat(properties, "Roughness", "_Roughness");
			AddFloat(properties, "Metallic", "_Metallic");
		}
		
		protected override Material GetMaterial => 
			TryRead<bool>("Use Alpha", out var property) && property ?
				UBFSettings.GetOrCreateSettings().Hair :
				UBFSettings.GetOrCreateSettings().PbrOpaque;
	}
}