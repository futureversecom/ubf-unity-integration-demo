// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
    public class FilterSceneNodes : ACustomExecNode
    {
        public FilterSceneNodes(Context context) : base(context) { }

        protected override void ExecuteSync()
        {
            if (!TryReadArray<SceneNode>("Scene Nodes", out var sceneNodes))
            {
                UbfLogger.LogError("[FilterSceneNodes] Could not find input \"Scene Nodes\"");
                return;
            }

            if (!TryRead<string>("Filter", out var filter))
            {
                UbfLogger.LogError("[FilterSceneNodes] Could not find input \"Filter\"");
                return;
            }

            // TODO implement and filter
            
            // Is this treating scene nodes as multiple heads of a tree, or is it just filtering within the array? 
            // assume filtering within the array

            var filtered = sceneNodes.Where(x => x.Name.ToLower().Contains(filter.ToLower()));
            WriteOutput("Filtered", filtered.ToArray());

        }
    }
}