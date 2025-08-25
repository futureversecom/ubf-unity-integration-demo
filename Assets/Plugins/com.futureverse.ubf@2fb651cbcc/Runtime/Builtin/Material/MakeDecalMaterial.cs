// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Futureverse.UBF.Runtime.Settings;
using Material = UnityEngine.Material;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class MakeDecalMaterial : MakeMaterialBase
	{
		public MakeDecalMaterial(Context context) : base(context) { }

		protected override void AddProperties(Dictionary<string, object> properties)
		{
			AddRenderMode(properties, "RenderMode", "_RenderMode");
			AddColor(properties, "BaseColor", "_Tint");
			AddBool(properties, "UseAlpha", "_UseAlpha");
			AddBool(properties, "UseAlphaTexture", "_UseAlphaTex");
			AddFloat(properties, "FresnelIOR", "_Fresnel_IOR");
			AddFloat(properties, "Opacity", "_Opacity");
			AddTexture(properties, "AlphaTexture", "_AlphaTex");

			AddBool(properties, "UseEmission", "_USEEMISSION");
			AddBool(properties, "UseEmissiveTint", "_UseEmissiveTint");
			AddTexture(properties, "EmissiveTexture", "_EmissiveTex");
			AddFloat(properties, "EmissiveColorBoost", "_EmissiveColorBoost");
			AddColor(properties, "EmissiveTint", "_EmissiveTint");
			AddFloat(properties, "EmissiveTintBoost", "_EmissiveTintBoost");

			AddBool(properties, "UseNormalMap", "_USENORMALMAP");
			AddBool(properties, "UseORM", "_USEORM");
			AddFloat(properties, "Occlusion", "_Occlusion");
			AddFloat(properties, "Roughness", "_Roughness");
			AddFloat(properties, "Metallic", "_Metallic");
			AddBool(properties, "UseDecals", "_USEDECALS");
			AddColor(properties, "TintBase", "_TintBase");
			AddFloat(properties, "RoughBase", "_RoughBase");
			AddFloat(properties, "DarkenBase", "_DarkenBase");
			AddFloat(properties, "MetalBase", "_MetalBase");
			AddFloat(properties, "FlakesBase", "_FlakesBase");
			AddColor(properties, "TintA", "_TintA");
			AddFloat(properties, "DarkenA", "_DarkenA");
			AddFloat(properties, "RoughA", "_RoughA");
			AddFloat(properties, "MetalA", "_MetalA");
			AddFloat(properties, "FlakesA", "_FlakesA");
			AddColor(properties, "TintB", "_TintB");
			AddFloat(properties, "DarkenB", "_DarkenB");
			AddFloat(properties, "RoughB", "_RoughB");
			AddFloat(properties, "MetalB", "_MetalB");
			AddFloat(properties, "FlakesB", "_FlakesB");
			AddColor(properties, "TintC", "_TintC");
			AddFloat(properties, "DarkenC", "_DarkenC");
			AddFloat(properties, "RoughC", "_RoughC");
			AddFloat(properties, "MetalC", "_MetalC");
			AddFloat(properties, "FlakesC", "_FlakesC");
			AddTexture(properties, "DiffuseTexture", "_DiffuseTex");
			AddTexture(properties, "ORM", "_ORMTex");
			AddTexture(properties, "DecalTexture", "_DecalTex");
			AddTexture(properties, "NormalTexture", "_NormalTex");
		}

		protected override Material GetMaterial => 
			TryRead<bool>("Use Alpha", out var property) && property ?
				UBFSettings.GetOrCreateSettings().DecalTransparent :
				UBFSettings.GetOrCreateSettings().DecalOpaque;
	}
}