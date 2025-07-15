// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;

namespace Futureverse.UBF.Runtime.Utils
{
	public static class DynamicUtils
	{
		/// <summary>
		/// Transforms a dictionary of dynamic data into a dictionary of C# objects. Useful to avoid having to work
		/// directly with dynamic data.
		/// </summary>
		/// <param name="dynamicDictionary">Original dictionary of dynamic data.</param>
		/// <returns>C# object dictionary.</returns>
		public static Dictionary<string, object> ToObjectDictionary(Dictionary<string, Dynamic> dynamicDictionary)
		{
			var objectDictionary = new Dictionary<string, object>();
			foreach (var (key, dynamic) in dynamicDictionary)
			{
				objectDictionary.Add(key, dynamic.AsObject());
			}

			return objectDictionary;
		}

		/// <summary>
		/// Turns a dictionary of C# objects into a dictionary of dynamic data
		/// </summary>
		/// <param name="objectDictionary">The original object dictionary.</param>
		/// <returns>Dictionary of dynamic data.</returns>
		public static Dictionary<string, Dynamic> ToDynamicDictionary(Dictionary<string, object> objectDictionary)
		{
			var dynamicDictionary = new Dictionary<string, Dynamic>();
			foreach (var (key, obj) in objectDictionary)
			{
				dynamicDictionary.Add(key, Dynamic.From(obj));
			}

			return dynamicDictionary;
		}

		/// <summary>
		/// Attempts to interpret dynamic data as an object. Tries to cast to bool, float, int, string, and finally,
		/// just a general object. If any succeed, returns that object.
		/// </summary>
		/// <param name="dynamic">The dynamic data to cast to an object.</param>
		/// <returns>The resulting C# object, or null if all casts failed.</returns>
		public static object AsObject(this Dynamic dynamic)
		{
			if (dynamic.TryReadBoolean(out var b))
			{
				return b;
			}

			if (dynamic.TryReadFloat(out var f))
			{
				return f;
			}

			if (dynamic.TryReadInt(out var i))
			{
				return i;
			}

			if (dynamic.TryReadString(out var s))
			{
				return s;
			}

			if (dynamic.TryInterpretAs<object>(out var o))
			{
				return o;
			}

			return null;
		}
	}
}