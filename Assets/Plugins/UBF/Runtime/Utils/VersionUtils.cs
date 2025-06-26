// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Utils;
using Newtonsoft.Json;

namespace Futureverse.UBF.Runtime
{
	public static class VersionUtils
	{
		public static readonly Version MinSupportedStandardVersion = Version.Parse("0.2.0");
		public static readonly Version MaxSupportedStandardVersion = Version.Parse("0.3.0");
		
		public static bool IsSupported(this Version version)
			=> version >= MinSupportedStandardVersion &&
				version <= MaxSupportedStandardVersion;

		public static IEnumerable<string> EnumerateMinorVersions(Version min, Version max)
		{
			if (min.Major != max.Major)
			{
				UbfLogger.LogWarn($"Cannot enumerate major Standard Versions {min} to {max}");
				yield break;
			}

			if (min < MinSupportedStandardVersion || max > MaxSupportedStandardVersion)
			{
				UbfLogger.LogWarn($"Cannot enumerate Standard Versions that are not supported");
				yield break;
			}

			var major = min.Major;
			for (var minor = min.Minor; minor <= max.Minor; minor++)
			{
				yield return $"{major}.{minor}.0";
			}
		}
	}
}