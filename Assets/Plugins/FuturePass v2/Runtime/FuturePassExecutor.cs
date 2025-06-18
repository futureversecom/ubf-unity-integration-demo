using UnityEngine;
using Newtonsoft.Json;
using Auth = Futureverse.FuturePass.FuturePassAuthentication;

#if UNITY_EDITOR
using UnityEditor;

namespace Futureverse.FuturePass
{
    [CustomEditor(typeof(FuturePassExecutor))]
    public class FuturePassExecutorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            var mg = (FuturePassExecutor)target;
            if (GUILayout.Button("Start Login"))
            {
                mg.StartLogin();
            }

            if (GUILayout.Button("Abort Login"))
            {
                mg.AbortLogin();
            }

            if (GUILayout.Button("Refresh Token"))
            {
                mg.RefreshToken();   
            }

            if (GUILayout.Button("Cache Refresh Token"))
            {
                mg.CacheRefreshToken();
            }

            if (GUILayout.Button("Login From Cached Token"))
            {
                mg.LoginFromCachedRefreshToken();
            }

            EditorGUILayout.Space();
        
            if (Auth.LoadedAuthenticationDetails != null)
            {
                var json = JsonConvert.SerializeObject(Auth.LoadedAuthenticationDetails, Formatting.Indented);
                EditorGUILayout.TextArea(json);
            }
            else
            {
                EditorGUILayout.TextArea("\n\n");
            }
        }
    }
}

#endif

namespace Futureverse.FuturePass
{
    public class FuturePassExecutor : MonoBehaviour
    {
        public Auth.Environment environment;
        public bool cacheRefreshToken;

        private void Start()
        {
            Auth.SetEnvironment(environment);
            Auth.SetTokenAutoCache(cacheRefreshToken);
        }

        public void StartLogin()
        {
            Auth.StartLogin();
        }

        public void AbortLogin()
        {
            Auth.AbortLogin();
        }

        public void RefreshToken()
        {
            Auth.RefreshToken();
        }

        public void CacheRefreshToken()
        {
            Auth.CacheRefreshToken();
        }

        public void LoginFromCachedRefreshToken()
        {
            Auth.LoginFromCachedRefreshToken();
        }
    }
}

