// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Futureverse.UBF.Runtime.Builtin
{
	public class PlayAnimation : ACustomNode
	{
		public PlayAnimation(Context context) : base(context) { }

		protected override void ExecuteSync()
		{
			if (!TryRead<Transform>("Animator", out var animator))
			{
				Debug.LogError("No animator supplied to PlayAnimation");
				TriggerNext();
				return;
			}

			if (!animator.TryGetComponent<GLBReference>(out var glbRef) || glbRef.GLTFImport == null)
			{
				Debug.LogError("No GLB Reference on animator, or invalid GLTFImport object");
				TriggerNext();
				return;
			}

			if (!animator.TryGetComponent<Animation>(out var anim))
			{
				Debug.LogError("No animation component on animator");
				TriggerNext();
				return;
			}

			TryRead<string>("Animation Name", out var animName);
			if (string.IsNullOrEmpty(animName))
			{
				Debug.LogError("Null animation name supplied to PlayAnimation");
				TriggerNext();
				return;
			}

			var glb = glbRef.GLTFImport;
			var clips = glb.GetAnimationClips();
			if (clips == null || clips.Length == 0)
			{
				Debug.LogError("No valid clips to run on GLB");
				TriggerNext();
				return;
			}

			var clip = clips.FirstOrDefault(x => x.name == animName);
			if (clip == null)
			{
				Debug.LogError("No matching clip on GLB");
				TriggerNext();
				return;
			}

			TryRead<bool>("Loop?", out var doLoop);
			anim.wrapMode = doLoop ? WrapMode.Loop : WrapMode.Default;
			anim.clip = clip;
			anim.Play();

			WriteOutput("Clip Time", Dynamic.Float(clip.length));
			TriggerNext();
		}
	}
}