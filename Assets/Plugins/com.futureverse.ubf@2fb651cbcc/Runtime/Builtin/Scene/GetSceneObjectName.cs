// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
    public class GetSceneObjectName : ACustomExecNode
    {
        public GetSceneObjectName(Context context) : base(context) { }

        protected override void ExecuteSync()
        {
            if (!TryRead<ISceneObject>("SceneObject", out var obj))
            {
                UbfLogger.LogError("[GetName] Could not find input \"Object\"");
                return;
            }

            
            WriteOutput("Name", obj?.GetName() ?? string.Empty);
        }
    }
}