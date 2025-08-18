// Copyright (c) 2025, Futureverse Corporation Limited. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using Futureverse.UBF.Runtime.Utils;
using UnityEngine;

namespace Futureverse.UBF.Runtime.Builtin
{
    public class FilterSceneComponents : ACustomExecNode
    {
        public FilterSceneComponents(Context context) : base(context) { }

        protected override void ExecuteSync()
        {
            if (!TryReadArray<SceneComponent>("Scene Components", out var components))
            {
                UbfLogger.LogError("[FilterSceneComponents] Could not find input \"Scene Components\"");
                return;
            }

            if (!TryRead<string>("Filter", out var filter))
            {
                UbfLogger.LogError("[FilterSceneComponents] Could not find input \"Filter\"");
                return;
            }

            // TODO implement and filter
            
            // Do I filter by component specific logic like mesh name? Or by name of scene node?
            var filtered = components.Where(x => x.GetFilterString().ToLower().Contains(filter.ToLower()));
            WriteOutput("Filtered", filtered.ToArray());

        }
    }
}