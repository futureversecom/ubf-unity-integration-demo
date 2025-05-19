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
			AddRenderMode(properties, "Render Mode", "_RenderMode");
			AddColor(properties, "Base Color", "_Tint");
			AddBool(properties, "Use Alpha", "_UseAlpha");
			AddBool(properties, "Use Alpha Tex", "_UseAlphaTex");
			AddFloat(properties, "Fresnel_IOR", "_Fresnel_IOR");
			AddFloat(properties, "Opacity", "_Opacity");
			AddTexture(properties, "Alpha Tex", "_AlphaTex");

			AddBool(properties, "Use Emission", "_USEEMISSION");
			AddBool(properties, "Use Emissive Tint", "_UseEmissiveTint");
			AddTexture(properties, "Emissive Tex", "_EmissiveTex");
			AddFloat(properties, "Emissive Color Boost", "_EmissiveColorBoost");
			AddColor(properties, "Emissive Tint", "_EmissiveTint");
			AddFloat(properties, "Emissive Tint Boost", "_EmissiveTintBoost");

			AddBool(properties, "Use Normal Map", "_USENORMALMAP");
			AddBool(properties, "Use ORM", "_USEORM");
			AddFloat(properties, "Occlusion", "_Occlusion");
			AddFloat(properties, "Roughness", "_Roughness");
			AddFloat(properties, "Metallic", "_Metallic");
			AddBool(properties, "Use Decals", "_USEDECALS");
			AddColor(properties, "Tint Base", "_TintBase");
			AddFloat(properties, "Rough Base", "_RoughBase");
			AddFloat(properties, "Darken Base", "_DarkenBase");
			AddFloat(properties, "Metal Base", "_MetalBase");
			AddFloat(properties, "Flakes Base", "_FlakesBase");
			AddColor(properties, "Tint A", "_TintA");
			AddFloat(properties, "Darken A", "_DarkenA");
			AddFloat(properties, "Rough A", "_RoughA");
			AddFloat(properties, "Metal A", "_MetalA");
			AddFloat(properties, "Flakes A", "_FlakesA");
			AddColor(properties, "Tint B", "_TintB");
			AddFloat(properties, "Darken B", "_DarkenB");
			AddFloat(properties, "Rough B", "_RoughB");
			AddFloat(properties, "Metal B", "_MetalB");
			AddFloat(properties, "Flakes B", "_FlakesB");
			AddColor(properties, "Tint C", "_TintC");
			AddFloat(properties, "Darken C", "_DarkenC");
			AddFloat(properties, "Rough C", "_RoughC");
			AddFloat(properties, "Metal C", "_MetalC");
			AddFloat(properties, "Flakes C", "_FlakesC");
			AddTexture(properties, "Diffuse Texture", "_DiffuseTex");
			AddTexture(properties, "ORM", "_ORMTex");
			AddTexture(properties, "Decal Tex", "_DecalTex");
			AddTexture(properties, "Normal Tex", "_NormalTex");
		}

		protected override Material GetMaterial => 
			TryRead<bool>("Use Alpha", out var property) && property ?
				UBFSettings.GetOrCreateSettings().DecalTransparent :
				UBFSettings.GetOrCreateSettings().DecalOpaque;
	}
}