// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Linq;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class FindRenderer : ACustomNode
	{
		public FindRenderer(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryReadArray<Renderer>("Array", out var array))
			{
				UbfLogger.LogError("[FindRenderer] Could not find input \"Array\"");
				return;
			}

			if (!TryRead<string>("Name", out var name))
			{
				UbfLogger.LogError("[FindRenderer] Could not find input \"Name\"");
				return;
			}

			var renderer = array.FirstOrDefault(x => x.name == name);
			WriteOutput("Renderer", renderer);
		}
	}
}