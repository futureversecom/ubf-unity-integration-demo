// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Futureverse.UBF.Runtime
{
    [CreateAssetMenu(fileName = "MeshConfig", menuName = "UBF/Mesh Config")]
    public class MeshConfig : ScriptableObject
    {
        public GameObject RigPrefab => _rigPrefab;
        public Avatar Avatar;

        [SerializeField]
        private GameObject _rigPrefab;
        
        public List<ConfigMapItem> avatarMap = new();

        [Serializable]
        public class ConfigMapItem
        {
            public string targetBoneName;
            public string sourceBoneName;

            public static Dictionary<string, string> ToDictionary(IEnumerable<ConfigMapItem> items)
            {
                return items.ToDictionary(item => item.targetBoneName, item => item.sourceBoneName);
            }
        }

        [ContextMenu("Copy Map Json")]
        public void CopyMapJson()
        {
            GUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(avatarMap, Formatting.Indented);
        }

        [ContextMenu("Generate avatar map")]
        public void GenerateAvatarMap()
        {
            avatarMap.Clear();
            foreach (var bone in Avatar.humanDescription.human)
            {
                var mapItem = new ConfigMapItem()
                {
                    sourceBoneName = bone.humanName,
                    targetBoneName = bone.boneName
                };
                avatarMap.Add(mapItem);
            }
        }
    }

    public class RuntimeMeshConfig
    {
        public MeshConfig Config;
        public GameObject AnimationObject;
        public GameObject GameLogicObject;
    }
}