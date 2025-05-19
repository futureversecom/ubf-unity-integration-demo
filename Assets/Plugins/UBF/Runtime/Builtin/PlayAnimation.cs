// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Linq;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class PlayAnimation : ACustomExecNode
	{
		public PlayAnimation(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<Transform>("Animator", out var animator))
			{
				UbfLogger.LogError("[PlayAnimation] Could not find input \"Animator\"");
				return;
			}

			if (!animator.TryGetComponent<GLBReference>(out var glbRef) || glbRef.GLTFImport == null)
			{
				UbfLogger.LogError("[PlayAnimation] No GLB Reference on animator, or invalid GLTFImport object");
				return;
			}

			if (!animator.TryGetComponent<Animation>(out var anim))
			{
				UbfLogger.LogError("[PlayAnimation] No Animation component on animator");
				return;
			}
			
			if (!TryRead<string>("Animation Name", out var animName) || string.IsNullOrEmpty(animName))
			{
				UbfLogger.LogError("[PlayAnimation] Could not find input \"Animation Name\"");
				return;
			}

			var glb = glbRef.GLTFImport;
			var clips = glb.GetAnimationClips();
			if (clips == null || clips.Length == 0)
			{
				UbfLogger.LogError("[PlayAnimation] No valid clips to run on GLB");
				return;
			}

			var clip = clips.FirstOrDefault(x => x.name == animName);
			if (clip == null)
			{
				UbfLogger.LogError("[PlayAnimation] No matching clip on GLB");
				return;
			}

			anim.wrapMode = TryRead<bool>("Loop?", out var doLoop) && doLoop ? WrapMode.Loop : WrapMode.Default;
			anim.clip = clip;
			anim.Play();

			WriteOutput("Clip Time", clip.length);
		}
	}
}