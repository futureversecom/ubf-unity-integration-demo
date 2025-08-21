// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
    public class GetChildSceneNodes : ACustomExecNode
    {
        public GetChildSceneNodes(Context context) : base(context) { }

        protected override void ExecuteSync()
        {
            if (!TryRead<SceneNode>("SceneNode", out var node))
            {
                UbfLogger.LogError("[GetChildSceneNodes] Could not find input \"Scene Node\"");
                return;
            }
            
            WriteOutput("Children", node.Children.ToArray());
        }
    }
}