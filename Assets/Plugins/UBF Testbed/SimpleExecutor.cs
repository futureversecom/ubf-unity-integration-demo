using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Execution;
using Futureverse.UBF.Runtime.Resources;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Testbed
{
    #if UNITY_EDITOR
    [CustomEditor(typeof(SimpleExecutor))]

    public class SimpleExecutorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(18);
            if (GUILayout.Button("Run"))
            {
                SimpleExecutor executor = (SimpleExecutor)target;
                executor.Run();
            }
        }
    }
    #endif
    
    public class SimpleExecutor : MonoBehaviour
    {
        [SerializeField] private string _blueprintUri;
        [SerializeField] private string _metadata;

        [ContextMenu("Run")]
        public void Run()
        {
            StartCoroutine(RunRoutine());
        }

        private IEnumerator RunRoutine()
        {
            var artifactProvider = new ArtifactProvider();
            var catalog = new Catalog();
            catalog.AddResource(new ResourceData("bp", _blueprintUri));
            artifactProvider.RegisterCatalog(catalog);
            var blueprintDefinition = new BlueprintInstanceData("bp");
            blueprintDefinition.AddInput("metadata", _metadata);

            yield return UBFExecutor.ExecuteRoutine(
                new ExecutionData(
                    transform,
                    (result =>
                    {
                        foreach (var r in result.BlueprintOutputs)
                        {
                            Debug.Log($"[{r.Key}]: {r.Value}");
                        }
                    }),
                    new List<IBlueprintInstanceData>
                    {
                        blueprintDefinition,
                    },
                    artifactProvider
                ),
                blueprintDefinition.InstanceId
            );
        }
    }
}