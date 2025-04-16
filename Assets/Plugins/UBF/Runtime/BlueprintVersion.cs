// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using Newtonsoft.Json;

namespace Futureverse.UBF.Runtime
{
	/// <summary>
	/// Represents a semver compatible version number.
	/// </summary>
	public class BlueprintVersion
		: IComparable<BlueprintVersion>, IComparable<string>, IEquatable<BlueprintVersion>, IEquatable<string>
	{
		private readonly int _major;
		private readonly int _minor;
		private readonly int _patch;

		/// <summary>
		/// Turns a semver compatible string into a BlueprintVersion object.
		/// </summary>
		/// <param name="version">The semver string.</param>
		/// <returns>The resulting BlueprintVersion object.</returns>
		public static BlueprintVersion FromString(string version)
		{
			var (major, minor, patch) = ParseString(version);
			if (major < 0 || minor < 0 || patch < 0)
			{
				return null;
			}

			return new BlueprintVersion(major, minor, patch);
		}

		private BlueprintVersion(int major, int minor, int patch)
		{
			_major = major;
			_minor = minor;
			_patch = patch;
		}

		private static (int, int, int) ParseString(string versionString)
		{
			try
			{
				var parts = versionString.Split('.');
				var major = int.Parse(parts[0]);
				var minor = int.Parse(parts[1]);
				var patch = int.Parse(parts[2]);
				return (major, minor, patch);
			}
			catch (Exception)
			{
				return (-1, -1, -1);
			}
		}

		/// <summary>
		/// Returns a semver string based on this BlueprintVersion.
		/// </summary>
		/// <returns>SemVer string.</returns>
		public override string ToString()
			=> $"{_major}.{_minor}.{_patch}";

		/// <summary>
		/// Returns whether the given BlueprintVersion is supported in this version of the UBF plugin. 
		/// </summary>
		/// <returns></returns>
		public bool IsSupported()
			=> this >= FromString(GraphVersionUtils.MinSupportedStandardVersion) &&
				this <= FromString(GraphVersionUtils.MaxSupportedStandardVersion);

		public int CompareTo(BlueprintVersion other)
		{
			if (other is null)
			{
				return 1;
			}

			if (_major != other._major)
			{
				return _major - other._major;
			}

			if (_minor == other._minor)
			{
				return _patch - other._patch;
			}

			return _minor - other._minor;
		}

		public bool Equals(BlueprintVersion other)
			=> other != null && _major == other._major && _minor == other._minor && _patch == other._patch;

		public override bool Equals(object obj)
			=> obj is BlueprintVersion other && Equals(other);

		public int CompareTo(string other)
			=> CompareTo(FromString(other));

		public bool Equals(string other)
			=> Equals(FromString(other));

		public override int GetHashCode()
			=> HashCode.Combine(_major, _minor, _patch);

		public static bool operator ==(BlueprintVersion left, BlueprintVersion right)
		{
			if (ReferenceEquals(left, right))
			{
				return true;
			}

			if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
			{
				return false;
			}

			return left.Equals(right);
		}

		public static bool operator !=(BlueprintVersion left, BlueprintVersion right)
			=> !(left == right);

		public static bool operator >(BlueprintVersion left, BlueprintVersion right)
		{
			if (ReferenceEquals(left, null))
			{
				return false;
			}

			if (ReferenceEquals(right, null))
			{
				return true;
			}

			return left.CompareTo(right) > 0;
		}

		public static bool operator <(BlueprintVersion left, BlueprintVersion right)
		{
			if (ReferenceEquals(right, null))
			{
				return false;
			}

			if (ReferenceEquals(left, null))
			{
				return true;
			}

			return left.CompareTo(right) < 0;
		}

		public static bool operator >=(BlueprintVersion left, BlueprintVersion right)
			=> left == right || left > right;

		public static bool operator <=(BlueprintVersion left, BlueprintVersion right)
			=> left == right || left < right;
	}

	internal static class GraphVersionUtils
	{
		public const string MinSupportedStandardVersion = "0.2.0";
		public const string MaxSupportedStandardVersion = "0.2.0";
		
		[JsonObject]
		private struct VersionedJson
		{
			[JsonProperty("version")]
			public string Version;
		}

		public static bool JsonHasSupportedVersion(string blueprintJson)
		{
			try
			{
				var versionedJson = JsonConvert.DeserializeObject<VersionedJson>(blueprintJson);
				return BlueprintVersion.FromString(versionedJson.Version)
					?.IsSupported() ?? false;
			}
			catch
			{
				return false;
			}
		}
	}
}