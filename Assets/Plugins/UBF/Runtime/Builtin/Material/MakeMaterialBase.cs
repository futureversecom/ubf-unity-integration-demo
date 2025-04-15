// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class MaterialValue
	{
		public Material Material;
		public Dictionary<string, object> Properties;
	}
	
	public abstract class MakeMaterialBase : ACustomNode
	{
		protected MakeMaterialBase(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			var properties = new Dictionary<string, object>();
			AddProperties(properties);
			var mat = UnityEngine.Resources.Load(ShaderPath) as Material;
			var shader = new MaterialValue
			{
				Material = mat,
				Properties = properties,
			};

			WriteOutput("Material", Dynamic.Foreign(shader));
		}

		/// <returns>The resources path of the shader to load</returns>
		protected abstract void AddProperties(Dictionary<string, object> dictionary);

		protected abstract string ShaderPath { get; }

		protected void AddRenderMode(
			Dictionary<string, object> propertiesDictionary,
			string resourceName,
			string propertyName)
		{
			if (!TryRead<string>(resourceName, out var property))
			{
				propertiesDictionary.Add(propertyName, 0);
				return;
			}

			propertiesDictionary.Add(
				propertyName,
				property switch
				{
					"UseDiffuse" => 0,
					"SolidColor" => 1,
					_ => 2,
				}
			);
		}

		protected void AddColor(
			Dictionary<string, object> propertiesDictionary,
			string resourceName,
			string propertyName)
		{
			if (!TryRead<string>(resourceName, out var property))
			{
				return;
			}

			if (ColorUtility.TryParseHtmlString(property, out var color))
			{
				propertiesDictionary.Add(propertyName, color);
			}
		}

		protected void AddFloat(
			Dictionary<string, object> propertiesDictionary,
			string resourceName,
			string propertyName)
		{
			AddProperty<float>(propertiesDictionary, resourceName, propertyName);
		}

		protected void AddInt(Dictionary<string, object> propertiesDictionary, string resourceName, string propertyName)
		{
			AddProperty<int>(propertiesDictionary, resourceName, propertyName);
		}

		protected void AddBool(
			Dictionary<string, object> propertiesDictionary,
			string resourceName,
			string propertyName)
		{
			AddProperty<bool>(propertiesDictionary, resourceName, propertyName);
		}

		private void AddProperty<T>(
			Dictionary<string, object> propertiesDictionary,
			string resourceName,
			string propertyName)
		{
			if (!TryRead<T>(resourceName, out var property))
			{
				return;
			}

			propertiesDictionary.Add(propertyName, property);
		}

		protected void AddTexture(
			Dictionary<string, object> dictionary,
			string resourceName,
			string propertyName,
			bool isNormal = false)
		{
			if (!TryReadResourceId(resourceName, out var resourceId) || !resourceId.IsValid)
			{
				return;
			}

			dictionary.Add(propertyName, resourceId);
		}
	}
}