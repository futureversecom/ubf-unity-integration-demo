using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineSceneObject : MonoBehaviour
{
    private static CoroutineSceneObject s_instance;
    
    public static CoroutineSceneObject Instance
    {
        get
        {
            if (s_instance != null)
            {
                return s_instance;
            }

            var existingController = FindFirstObjectByType<CoroutineSceneObject>();
            if (existingController != null)
            {
                s_instance = existingController;
            }
            else
            {
                var newGo = new GameObject("Coroutine Scene Object");
                s_instance = newGo.AddComponent<CoroutineSceneObject>();
            }

            return s_instance;
        }
    }
}
