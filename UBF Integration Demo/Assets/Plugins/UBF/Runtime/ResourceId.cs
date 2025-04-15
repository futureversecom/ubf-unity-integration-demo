// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

namespace Futureverse.UBF.Runtime
{
	/// <summary>
	/// Wrapper around string IDs for resources that are read from the Blueprints at runtime.
	/// </summary>
	public struct ResourceId
	{
		/// <summary>
		/// The underlying resource ID string.
		/// </summary>
		public string Value;
		/// <summary>
		/// If the Resource ID was successfully read from the Blueprint.
		/// </summary>
		public readonly bool IsValid => !string.IsNullOrEmpty(Value);

		/// <summary>
		/// Creates a new Resource ID from a raw string. Not recommended unless strictly necessary, as Resource IDs
		/// should always come from the Blueprint itself.
		/// </summary>
		/// <param name="value">String to use as the underlying Resource ID.</param>
		/// <returns>Newly created ResourceID object.</returns>
		public static ResourceId UnsafeFromString(string value)
			=> new()
			{
				Value = value,
			};

		public readonly override bool Equals(object obj)
			=> obj is ResourceId key && Value == key.Value;

		public readonly override int GetHashCode()
			=> Value.GetHashCode();
	}
}