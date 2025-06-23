// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Testbed.Local
{
	public interface IBlueprintInput
	{
		string Key { get; }
		object GetValue { get; }
	}

	[Serializable]
	public class StringInput : IBlueprintInput
	{
		public object GetValue => _value;
		public string Key => _key;
		
		[SerializeField] private string _key;
		[SerializeField] private string _value;
	}
	
	[Serializable]
	public class IntInput : IBlueprintInput
	{
		public object GetValue => _value;
		public string Key => _key;
		
		[SerializeField] private string _key;
		[SerializeField] private int _value;
	}
	
	[Serializable]
	public class FloatInput : IBlueprintInput
	{
		public object GetValue => _value;
		public string Key => _key;
		
		[SerializeField] private string _key;
		[SerializeField] private float _value;
	}
	
	[Serializable]
	public class BoolInput : IBlueprintInput
	{
		public object GetValue => _value;
		public string Key => _key;
		
		[SerializeField] private string _key;
		[SerializeField] private bool _value;
	}
	
	[Serializable]
	public class ObjectInput : IBlueprintInput
	{
		public object GetValue => _value;
		public string Key => _key;
		
		[SerializeField] private string _key;
		[SerializeField] private UnityEngine.Object _value;
	}
}