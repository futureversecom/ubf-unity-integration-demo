// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using Futureverse.UBF.Runtime.Utils;

namespace Futureverse.UBF.Runtime.Builtin
{
    public class GetSceneComponents : ACustomExecNode
    {
        public GetSceneComponents(Context context) : base(context) { }

        protected override void ExecuteSync()
        {
            if (!TryRead<SceneNode>("SceneNode", out var node))
            {
                UbfLogger.LogError("[GetSceneComponents] Could not find input \"Scene Node\"");
                return;
            }

            if (!TryRead<string>("Type", out var componentType))
            {
                UbfLogger.LogError("[GetSceneComponents] Could not find input \"Type\"");
                return;
            }
            
            var components = new List<SceneComponent>();
            switch (componentType)
            {
                case "MeshRenderer":
                    components.AddRange(node.GetComponents<MeshRendererSceneComponent>());
                    break;
                case "Rig":
                    components.AddRange(node.GetComponents<RigSceneComponent>());
                    break;
            }
            WriteOutput("SceneComponents", components.ToArray());
        }
    }
}