// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
    public class GetRigRootBone : ACustomExecNode
    {
        public GetRigRootBone(Context context) : base(context) { }

        protected override void ExecuteSync()
        {
            if (!TryRead<RigSceneComponent>("Rig", out var rig))
            {
                UbfLogger.LogError("[GetRigRootBone] Could not find input \"Rig\"");
                return;
            }

            
            WriteOutput("RootBone", rig.Root);
        }
    }
}