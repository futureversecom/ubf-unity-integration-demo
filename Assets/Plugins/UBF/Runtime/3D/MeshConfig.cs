// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using UnityEngine;

namespace Futureverse.UBF.Runtime
{
    [CreateAssetMenu(fileName = "MeshConfig", menuName = "UBF/Mesh Config")]
    public class MeshConfig : ScriptableObject
    {
        public GameObject RigPrefab => _rigPrefab;
        public Avatar Avatar => _avatar;

        [SerializeField]
        private GameObject _rigPrefab;
        [SerializeField]
        private Avatar _avatar;
    }

    public class RuntimeMeshConfig
    {
        public MeshConfig Config;
        public GameObject RuntimeObject;
    }
}