// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using GLTFast;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Random = UnityEngine.Random;

namespace Futureverse.UBF.Runtime
{
	/// <summary>
	/// Scene representation of an instantiated GLB / GLTFImport object
	/// </summary>
	public class GLBReference : MonoBehaviour
	{
		/// <summary>
		/// Object used to cache animation clips and view their properties in the Unity inspector
		/// </summary>
		[Serializable]
		public class PlayableClipDebug
		{
			public readonly AnimationClipPlayable Playable;
			
			[SerializeField] private AnimationClip _clip;
			[SerializeField] private bool _isDone;
			[SerializeField] private bool _isNull;
			[SerializeField] private bool _isValid;
			[SerializeField] private PlayState _playState;
			[SerializeField] private float _time;
			[SerializeField] private float _clipLength;

			public PlayableClipDebug(AnimationClipPlayable playable, AnimationClip clip)
			{
				Playable = playable;
				_clip = clip;
				_clipLength = clip.length;
			}
			
			public void Update()
			{
				_isDone = Playable.IsDone();
				_isNull = Playable.IsNull();
				_isValid = Playable.IsValid();
				_playState = Playable.GetPlayState();
				_time = (float)Playable.GetTime();
			}
		}

		public GltfImport GLTFImport;

		public List<PlayableClipDebug> PlayableClips = new();
		private PlayableGraph playableGraph;
		private Animator animator;
		private Coroutine loopRoutine;

		/// <summary>
		/// Create a basic PlayableGraph using animation clips found within the imported GLB
		/// </summary>
		public void CreatePlayableGraph()
		{
			var clips = GLTFImport.GetAnimationClips();
			CreatePlayableGraph(clips);
		}

		/// <summary>
		/// Create a playable graph from any set of animation clips. These can come from the imported GLB, or animation clips already located in the project.
		/// </summary>
		/// <param name="clips"></param>
		public void CreatePlayableGraph(AnimationClip[] clips)
		{
			PlayableClips.Clear();

			VerifyAnimator();

			playableGraph = PlayableGraph.Create();
			playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

			// Register output to graph
			AnimationPlayableOutput.Create(playableGraph, "Animation", animator);

			foreach (var clip in clips)
			{
				clip.legacy = false;

				var pClip = AnimationClipPlayable.Create(playableGraph, clip);
				PlayableClips.Add(
					new PlayableClipDebug(pClip, clip)
				);
			}
		}

		/// <summary>
		/// Ensures an animator is present on the object. Only called when animation relevant functions are called
		/// 
		/// </summary>
		private void VerifyAnimator()
		{
			if (!TryGetComponent(out animator))
			{
				animator = gameObject.AddComponent<Animator>();
			}
		}

		/// <summary>
		/// Play the currently loaded PlayableGraph
		/// </summary>
		public void Play()
		{
			if (loopRoutine != null)
			{
				StopCoroutine(loopRoutine);
			}

			playableGraph.Play();
		}

		/// <summary>
		/// Play a single AnimationClipPlayable, looping continuously
		/// </summary>
		/// <param name="playableClip"></param>
		public void PlayLooping(AnimationClipPlayable playableClip)
		{
			if (loopRoutine != null)
			{
				StopCoroutine(loopRoutine);
			}

			loopRoutine = StartCoroutine(LoopRoutine(playableClip));
		}

		/// <summary>
		/// Play a series of animations in a loop, choosing the next animation at random
		/// </summary>
		public void PlayRandomLoop()
		{
			if (loopRoutine != null)
			{
				StopCoroutine(loopRoutine);
			}

			loopRoutine = StartCoroutine(LoopRandomRoutine());
		}

		/// <summary>
		/// Stop any currently playing graph
		/// </summary>
		public void Stop()
		{
			if (loopRoutine != null)
			{
				StopCoroutine(loopRoutine);
			}

			playableGraph.Stop();
		}

		/// <summary>
		/// Loop a single clip forever. There should only ever be one of this routine. 
		/// </summary>
		/// <param name="playableClip"></param>
		/// <returns></returns>
		private IEnumerator LoopRoutine(AnimationClipPlayable playableClip)
		{
			var graphOutput = playableGraph.GetOutput(0);
			playableGraph.Stop();
			graphOutput.SetSourcePlayable(playableClip);
			playableGraph.Play();

			while (true)
			{
				playableClip.SetTime(0);
				for (float f = 0;
					f <
					playableClip.GetAnimationClip()
						.length;
					f += Time.deltaTime)
				{
					foreach (var clip in PlayableClips)
					{
						clip.Update();
					}

					yield return null;
				}
			}
		}

		/// <summary>
		/// Play the clips from the PlayableClips object on loop, choosing the next clip at random each time
		/// </summary>
		/// <returns></returns>
		private IEnumerator LoopRandomRoutine()
		{
			var playableClip = PlayableClips[Random.Range(0, PlayableClips.Count - 1)].Playable;
			var graphOutput = playableGraph.GetOutput(0);
			playableGraph.Stop();
			graphOutput.SetSourcePlayable(playableClip);
			playableGraph.Play();

			while (true)
			{
				playableClip.SetTime(0);
				for (float f = 0;
					f <
					playableClip.GetAnimationClip()
						.length;
					f += Time.deltaTime)
				{
					foreach (var clip in PlayableClips)
					{
						clip.Update();
					}

					yield return null;
				}

				playableClip = PlayableClips[Random.Range(0, PlayableClips.Count - 1)].Playable;
				graphOutput.SetSourcePlayable(playableClip);
			}
		}

		/// <summary>
		/// Play an animation clip on this object without constructing a playable graph beforehand
		/// </summary>
		/// <param name="clip"></param>
		public void PlayUtility(AnimationClip clip)
		{
			VerifyAnimator();
			AnimationPlayableUtilities.PlayClip(animator, clip, out playableGraph);
		}

		private void OnDestroy()
		{
			if (playableGraph.IsValid())
			{
				playableGraph.Destroy();
			}
		}
	}

	public class PlayableLoopingBehaviour : PlayableBehaviour { }
}