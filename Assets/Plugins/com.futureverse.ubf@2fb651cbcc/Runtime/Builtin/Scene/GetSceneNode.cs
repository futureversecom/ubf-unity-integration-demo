// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
    public class GetSceneNode : ACustomExecNode
    {
        public GetSceneNode(Context context) : base(context) { }

        protected override void ExecuteSync()
        {
            if (!TryRead<SceneComponent>("SceneComponent", out var component))
            {
                UbfLogger.LogError("[GetSceneNode] Could not find input \"Scene Component\"");
                return;
            }
            
            WriteOutput("SceneNode", component?.Node);
        }
    }
}