// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
    public class AttachSceneNode : ACustomExecNode
    {
        public AttachSceneNode(Context context) : base(context) { }

        protected override void ExecuteSync()
        {
            if (!TryRead<SceneNode>("SceneNode", out var node))
            {
                UbfLogger.LogError("[AttachSceneNode] Could find input \"SceneNode\"");
                return;
            }
            
            if (!TryRead<SceneNode>("Parent", out var parent))
            {
                UbfLogger.LogError("[AttachSceneNode] Could not find input \"Parent\"");
                return;
            }

            parent.AddChild(node, removeFromParent: true, reparentGameObjects: true);
        }
    }
}