// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using GLTFast;
using GLTFast.Logging;
using Unity.Collections;
using UnityEngine;

namespace Plugins.UBF.Runtime
{
	public class UbfMeshInstantiator :  GameObjectInstantiator
	{
		private readonly List<string> _validMeshNames;
		private readonly List<uint> _unusedNodes = new();

		public UbfMeshInstantiator(
			IGltfReadable gltf,
			Transform parent,
			List<string> validMeshNames = null,
			ICodeLogger logger = null,
			InstantiationSettings settings = null) : base(
			gltf,
			parent,
			logger,
			settings
		)
		{
			_validMeshNames = validMeshNames;
		}

		public override void AddPrimitive(
			uint nodeIndex,
			string meshName,
			MeshResult meshResult,
			uint[] joints = null,
			uint? rootJoint = null,
			float[] morphTargetWeights = null,
			int meshNumeration = 0)
		{
			if (_validMeshNames != null &&
				!_validMeshNames.Contains(meshName))
			{
				_unusedNodes.Add(nodeIndex);
				return;
			}
			
			base.AddPrimitive(
				nodeIndex,
				meshName,
				meshResult,
				joints,
				rootJoint,
				morphTargetWeights,
				meshNumeration
			);
		}

		public override void AddPrimitiveInstanced(
			uint nodeIndex,
			string meshName,
			MeshResult meshResult,
			uint instanceCount,
			NativeArray<Vector3>? positions,
			NativeArray<Quaternion>? rotations,
			NativeArray<Vector3>? scales,
			int meshNumeration = 0)
		{
			if (_validMeshNames != null &&
				!_validMeshNames.Contains(meshName))
			{
				_unusedNodes.Add(nodeIndex);
				return;
			}
			
			base.AddPrimitiveInstanced(
				nodeIndex,
				meshName,
				meshResult,
				instanceCount,
				positions,
				rotations,
				scales,
				meshNumeration
			);
		}

		public override void AddAnimation(AnimationClip[] animationClips)
		{
			// Ignore animations
		}

		public override void AddCamera(uint nodeIndex, uint cameraIndex)
		{
			// Ignore cameras
		}

		public override void EndScene(uint[] rootNodeIndices)
		{
			base.EndScene(rootNodeIndices);

			foreach (var nodeIndex in _unusedNodes)
			{
				if (m_Nodes.TryGetValue(nodeIndex, out var obj))
				{
					Object.Destroy(obj);
				}
			}
		}
	}
}