// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Futureverse.UBF.Runtime.Native.FFI;

namespace Futureverse.UBF.Runtime
{
	/// <summary>
	/// Represents internal UBF data that crosses the FFI into the core dll.
	/// </summary>
	public unsafe class Dynamic
	{
		internal readonly Native.FFI.Dynamic* NativePtr;
		private readonly DynamicType _type;

		private string _asString;
		private readonly bool _asBoolean;
		private readonly float _asFloat;
		private readonly int _asInt;
		private readonly IntPtr _asPtr;

		internal Dynamic(Native.FFI.Dynamic* ptr)
		{
			DynamicType tmpType;
			int tmpInt;
			float tmpFloat;
			IntPtr tmpPtr;
			Calls.dynamic_extract(
				ptr,
				&tmpType,
				&tmpInt,
				&tmpFloat,
				&tmpPtr
			);
			NativePtr = ptr;
			_type = tmpType;
			_asFloat = tmpFloat;
			_asInt = tmpInt;
			_asPtr = tmpPtr;
			_asBoolean = tmpInt != 0;
		}

		internal Dynamic(
			Native.FFI.Dynamic* nativePtr,
			DynamicType type,
			bool asBoolean = false,
			float asFloat = 0,
			int asInt = 0,
			IntPtr asPtr = default)
		{
			NativePtr = nativePtr;
			this._type = type;
			this._asBoolean = asBoolean;
			this._asFloat = asFloat;
			this._asInt = asInt;
			this._asPtr = asPtr;
		}

		~Dynamic()
		{
			Calls.dynamic_release(NativePtr);
		}

		/// <summary>
		/// Attempts to turn the Dynamic value into a string.
		/// </summary>
		/// <returns>String value.</returns>
		public override string ToString()
		{
			void* mem = null;
			ushort* bytes = null;
			nuint bytesLen;
			if (!Calls.dynamic_to_string(
				NativePtr,
				&mem,
				&bytes,
				&bytesLen
			))
			{
				return "Dynamic";
			}

			var str = new string((char*)bytes, 0, (int)bytesLen);
			Calls.box_release(mem);
			return $"Dynamic({str})";
		}

		public delegate void ArrayIterator(Dynamic dynamic);

		/// <summary>
		/// If the underlying dynamic type is an array, perform a callback on each of the elements.. If not, do nothing.
		/// </summary>
		/// <param name="callback">Callback to perform on each of the elements of the array.</param>
		public void ForEach(ArrayIterator callback)
		{
			if (_type != DynamicType.Array)
			{
				return;
			}

			var output = new List<Dynamic>();
			var context = GCHandle.Alloc(output);
			Calls.dynamic_array_iter(NativePtr, (IntPtr)context, IterateArrayCallback);
			context.Free();

			foreach (var dynamic in output)
			{
				callback?.Invoke(dynamic);
			}
		}

		[MonoPInvokeCallback(typeof(Calls.dynamic_array_iter_iterator_delegate))]
		private static bool IterateArrayCallback(nint context, Native.FFI.Dynamic* defaultPtr)
		{
			var element = new Dynamic(defaultPtr);
			var xs = (List<Dynamic>)GCHandle.FromIntPtr(context)
				.Target;
			xs.Add(element);
			return true;
		}
		
		internal bool TryDeref<T>(out T value) where T : class
			=> TryDerefImpl(out value);

		private bool TryDerefImpl<T>(out T value)
		{
			if (_type != DynamicType.Foreign)
			{
				value = default(T);
				return false;
			}

			try
			{
				value = (T)GCHandle.FromIntPtr(_asPtr)
					.Target;
				return value != null;
			}
			catch (Exception)
			{
				value = default(T);
				return false;
			}
		}

		/// <summary>
		/// Creates a new dynamic data with an underlying string type.
		/// </summary>
		/// <param name="str">The string to turn into dynamic data.</param>
		/// <returns>The created dynamic object.</returns>
		public static Dynamic String(string str)
		{
			fixed (char* p = str)
			{
				return new Dynamic(Calls.dynamic_new_string((ushort*)p, str.Length), DynamicType.String);
			}
		}

		/// <summary>
		/// If the underlying dynamic type is an array, push an element onto the array.
		/// </summary>
		/// <param name="value">Element to append to the array.</param>
		/// <returns>If the element was successfully added to the array.</returns>
		public bool Push(Dynamic value)
			=> Calls.dynamic_array_push(NativePtr, value.NativePtr);

		/// <summary>
		/// If the underlying dynamic type is a Hashmap, get the element with a given key.
		/// </summary>
		/// <param name="key">Used to retrieve the dynamic data.</param>
		/// <param name="value">The dynamic data with the given key.</param>
		/// <returns>If an element with the given key was found.</returns>
		public bool TryGet(string key, out Dynamic value)
		{
			fixed (char* p = key)
			{
				Native.FFI.Dynamic* valuePtr;
				if (Calls.dynamic_dictionary_get(
					NativePtr,
					(ushort*)p,
					key.Length,
					&valuePtr
				))
				{
					value = new Dynamic(valuePtr);
					return true;
				}
			}

			value = null;
			return false;
		}

		/// <summary>
		/// If the underlying dynamic type is a Hashmap, add an element with a given key.
		/// </summary>
		/// <param name="key">Used to index the value in the hashmap.</param>
		/// <param name="value">The dynamic value to add to the hashmap.</param>
		/// <returns>If the element was successfully added.</returns>
		public bool TrySet(string key, Dynamic value)
		{
			fixed (char* keyBytes = key)
			{
				return Calls.dynamic_dictionary_set(
					NativePtr,
					(ushort*)keyBytes,
					key.Length,
					value.NativePtr
				);
			}
		}

		/// <summary>
		/// Try to get the underlying string value of the dynamic data.
		/// </summary>
		/// <param name="str">The resulting string data.</param>
		/// <returns>If the dynamic data had an underlying string value.</returns>
		public bool TryReadString(out string str)
		{
			if (_type != DynamicType.String)
			{
				str = null;
				return false;
			}

			if (_asString != null)
			{
				str = _asString;
				return true;
			}

			void* mem = null;
			ushort* bytes = null;
			nuint bytesLen;
			if (Calls.dynamic_as_string(
				NativePtr,
				&mem,
				&bytes,
				&bytesLen
			))
			{
				str = new string((char*)bytes, 0, (int)bytesLen);
				_asString = str;
				Calls.box_release(mem);
				return true;
			}

			str = null;
			return false;
		}

		/// <summary>
		/// Try to get the underlying bool value of the dynamic data.
		/// </summary>
		/// <param name="boolean">The resulting bool data.</param>
		/// <returns>If the dynamic data had an underlying bool value.</returns>
		public bool TryReadBoolean(out bool boolean)
		{
			if (_type != DynamicType.Bool)
			{
				boolean = false;
				return false;
			}

			boolean = _asBoolean;
			return true;
		}

		/// <summary>
		/// Try to get the underlying float value of the dynamic data.
		/// </summary>
		/// <param name="f">The resulting float data.</param>
		/// <returns>If the dynamic data had an underlying float value.</returns>
		public bool TryReadFloat(out float f)
		{
			if (_type != DynamicType.Float)
			{
				f = 0;
				return false;
			}

			f = _asFloat;
			return true;
		}

		/// <summary>
		/// Try to get the underlying int value of the dynamic data.
		/// </summary>
		/// <param name="i">The resulting int data.</param>
		/// <returns>If the dynamic data had an underlying int value.</returns>
		public bool TryReadInt(out int i)
		{
			if (_type != DynamicType.Int)
			{
				i = 0;
				return false;
			}

			i = _asInt;
			return true;
		}

		/// <summary>
		/// Try to get the underlying array value of the dynamic data.
		/// </summary>
		/// <param name="list">The resulting array data as a list.</param>
		/// <returns>If the dynamic data had an underlying array value.</returns>
		public bool TryReadArray<T>(out List<T> list)
		{
			var l = new List<T>();
			if (_type != DynamicType.Array)
			{
				list = l;
				return false;
			}

			ForEach(
				dynamic =>
				{
					if (dynamic.TryInterpretAs(out T element)) { }

					l.Add(element);
				}
			);

			list = l;
			return true;
		}

		/// <summary>
		/// Creates dynamic data with an underlying int value.
		/// </summary>
		/// <param name="i">Int value to turn into dynamic data.</param>
		/// <returns>The created dynamic data.</returns>
		public static Dynamic Int(int i)
			=> new(Calls.dynamic_new_primitive(DynamicType.Int, i, 0), DynamicType.Int, asInt: i);
		/// <summary>
		/// Creates dynamic data with an underlying bool value.
		/// </summary>
		/// <param name="b">Bool value to turn into dynamic data.</param>
		/// <returns>The created dynamic data.</returns>
		public static Dynamic Bool(bool b)
			=> new(Calls.dynamic_new_primitive(DynamicType.Bool, b ? 1 : 0, 0), DynamicType.Bool, b);
		/// <summary>
		/// Creates dynamic data with an underlying float value.
		/// </summary>
		/// <param name="f">Float value to turn into dynamic data.</param>
		/// <returns>The created dynamic data.</returns>
		public static Dynamic Float(float f)
			=> new(Calls.dynamic_new_primitive(DynamicType.Float, 0, f), DynamicType.Float, asFloat: f);
		/// <summary>
		/// Creates dynamic data with an underlying array value. The array is empty.
		/// </summary>
		/// <returns>The created dynamic data.</returns>
		public static Dynamic Array()
			=> new(Calls.dynamic_new_array(), DynamicType.Array);
		/// <summary>
		/// Creates dynamic data with an underlying hashmap value. The hashmap is empty.
		/// </summary>
		/// <returns>The created dynamic data.</returns>
		public static Dynamic Dictionary()
			=> new(Calls.dynamic_new_dictionary(), DynamicType.Dictionary);

		internal static Dynamic Foreign(object o)
		{
			var ptr = o == null ? (IntPtr)0 : (IntPtr)GCHandle.Alloc(o);
			return new Dynamic(Calls.dynamic_new_foreign(ptr, DropPointer), DynamicType.Foreign, asPtr: ptr);
		}

		[MonoPInvokeCallback(typeof(Calls.dynamic_new_foreign_drop_delegate))]
		private static void DropPointer(nint ptr)
		{
			try
			{
				GCHandle.FromIntPtr(ptr)
					.Free();
			}
			catch (InvalidOperationException)
			{
			}
		}

		/// <summary>
		/// Attempts to cast the underlying dynamic data into a given object.
		/// </summary>
		/// <param name="value">The resulting cast object.</param>
		/// <typeparam name="T">The type to cast the dynamic data to.</typeparam>
		/// <returns>If the cast was successful.</returns>
		public bool TryInterpretAs<T>(out T value)
		{
			if (typeof(T) == typeof(bool))
			{
				if (TryReadBoolean(out var v))
				{
					value = (T)(object)v;
					return true;
				}
			}
			else if (typeof(T) == typeof(float))
			{
				if (TryReadFloat(out var v))
				{
					value = (T)(object)v;
					return true;
				}
			}
			else if (typeof(T) == typeof(int))
			{
				if (TryReadInt(out var v))
				{
					value = (T)(object)v;
					return true;
				}
			}
			else if (typeof(T) == typeof(string))
			{
				if (TryReadString(out var v))
				{
					value = (T)(object)v;
					return true;
				}
			}
			else if (typeof(T) == typeof(ResourceId))
			{
				if (TryReadString(out var v))
				{
					value = (T)(object)ResourceId.UnsafeFromString(v);
					return true;
				}
			}

			// TODO: special case for dictionary & arrays? - would require the use of reflection to 
			// figure out the inner type. using TryReadArray for now
			else
			{
				return TryDerefImpl(out value);
			}

			value = default(T);
			return false;
		}

		public static explicit operator bool(Dynamic a)
		{
			if (a.TryReadBoolean(out var v))
			{
				return v;
			}

			throw new InvalidCastException();
		}

		public static explicit operator float(Dynamic a)
		{
			if (a.TryReadFloat(out var v))
			{
				return v;
			}

			throw new InvalidCastException();
		}

		public static explicit operator int(Dynamic a)
		{
			if (a.TryReadInt(out var v))
			{
				return v;
			}

			throw new InvalidCastException();
		}

		public static explicit operator string(Dynamic a)
		{
			if (a.TryReadString(out var v))
			{
				return v;
			}

			throw new InvalidCastException();
		}

		public static Dynamic From(object value)
		{
			switch (value)
			{
				case string str:
					return String(str);
				case bool b:
					return Bool(b);
				case float f:
					return Float(f);
				case int i:
					return Int(i);
				case ResourceId resourceId:
					return String(resourceId.Value);
				case Dictionary<string, object> dict:
				{
					var output = Dictionary();
					foreach (var (key, value1) in dict)
					{
						output.TrySet(key, From(value1));
					}

					return output;
				}
				case Dynamic d:
					return d;
				case IEnumerable<object> xs:
				{
					var array = Array();
					foreach (var item in xs)
					{
						array.Push(From(item));
					}

					return array;
				}
				default:
					return Foreign(value);
			}
		}
	}
}