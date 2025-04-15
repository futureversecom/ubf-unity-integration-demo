// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Linq;
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
				Debug.LogWarning("No renderers supplied to FindRenderer");
				return;
			}

			if (!TryRead<string>("Name", out var name))
			{
				Debug.LogWarning("No name provided to FindRenderer");
				return;
			}

			var renderer = array.FirstOrDefault(x => x.name == name);
			WriteOutput("Renderer", Dynamic.Foreign(renderer));
		}
	}
}