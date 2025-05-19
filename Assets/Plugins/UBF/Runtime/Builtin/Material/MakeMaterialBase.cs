// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Futureverse.UBF.Runtime.Utils;
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
			var shader = new MaterialValue
			{
				Material = GetMaterial,
				Properties = properties,
			};

			WriteOutput("Material", shader);
		}

		/// <summary>
		/// Use 'AddFloat', 'AddRenderMode' etc. methods to add properties to the dictionary parameter
		/// </summary>
		/// <param name="dictionary">The dictionary to add properties to</param>
		protected abstract void AddProperties(Dictionary<string, object> dictionary);

		/// <summary>
		/// Return the material that this node customizes properties for
		/// </summary>
		protected abstract Material GetMaterial { get; }

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
				UbfLogger.LogError($"[{GetType().Name}] Could not find input \"{resourceName}\"");
				return;
			}
			
			propertiesDictionary.Add(propertyName, property);
		}

		protected void AddTexture(
			Dictionary<string, object> dictionary,
			string resourceName,
			string propertyName)
		{
			if (TryReadResourceId(resourceName, out var resourceId) && resourceId.IsValid)
			{
				dictionary.Add(propertyName, resourceId);
			}
		}
	}
}