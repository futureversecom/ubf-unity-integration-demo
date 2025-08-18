// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using Futureverse.UBF.Runtime.Utils;

namespace Futureverse.UBF.Runtime.Builtin
{
    public class GetParentSceneNode : ACustomExecNode
    {
        public GetParentSceneNode(Context context) : base(context) { }

        protected override void ExecuteSync()
        {
            if (!TryRead<SceneNode>("Scene Node", out var node))
            {
                UbfLogger.LogError("[GetParentSceneNode] Could not find input \"Scene Node\"");
                return;
            }
            
            WriteOutput("Scene Node", node.Parent);
        }
    }
}