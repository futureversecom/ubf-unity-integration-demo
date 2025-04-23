#if UNITY_EDITOR

using System.Collections.Generic;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using UnityEditor;
using UnityEngine;
using Avatar = EmergenceSDK.Runtime.Types.Avatar;

namespace EmergenceSDK.Tests.Internal.EditorWindowDrivenTests
{
    public class AvatarTesting : BaseTestWindow
    {
        
        List<Avatar> avatars = new List<Avatar>();

        private void OnGUI()
        {
            if (!ReadyToTest(out var msg))
            {
                EditorGUILayout.LabelField(msg);
                return;
            }
            needsCleanUp = true;
            
            EditorGUILayout.LabelField("Test Avatar Service");

            if (GUILayout.Button("TestAvatarsByOwner"))
            {
                EmergenceServiceProvider.GetService<IAvatarService>().AvatarsByOwner(EmergenceServiceProvider.GetService<IWalletService>().WalletAddress, (avatarsIn) => avatars = avatarsIn, EmergenceLogger.LogError);
            }
            EditorGUILayout.LabelField("Retrieved Avatars:");
            foreach (var avatar in avatars)
            {
                EditorGUILayout.LabelField("Avatar: " + avatar.meta.name);
                EditorGUILayout.LabelField("Contract: " + avatar.contractAddress);
            }
        }

        protected override void CleanUp()
        {
            avatars.Clear();
        }
    }
}

#endif