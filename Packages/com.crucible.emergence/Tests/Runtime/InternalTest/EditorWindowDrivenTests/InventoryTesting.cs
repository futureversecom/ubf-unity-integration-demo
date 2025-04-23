#if UNITY_EDITOR

using System.Collections.Generic;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types.Inventory;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK.Tests.Internal.EditorWindowDrivenTests
{
    public class InventoryTesting : BaseTestWindow
    {
        
        private List<InventoryItem> inventoryItems;
        
        private void OnGUI()
        {
            if (!ReadyToTest(out var msg))
            {
                EditorGUILayout.LabelField(msg);
                return;
            }
            needsCleanUp = true;
            
            EditorGUILayout.LabelField("Test Inventory Service");
            
            if (GUILayout.Button("InventoryByOwner")) 
                EmergenceServiceProvider.GetService<IInventoryService>().InventoryByOwner(EmergenceServiceProvider.GetService<IWalletService>().WalletAddress, InventoryChain.AnyCompatible, (inventory) => inventoryItems = inventory, EmergenceLogger.LogError);
            
            EditorGUILayout.LabelField("Retrieved Inventory:");
            foreach (var item in inventoryItems)
            {
                EditorGUILayout.LabelField("Item: " + item.Meta.Name);
                EditorGUILayout.LabelField("Contract: " + item.Contract);
            }
        }

        protected override void CleanUp()
        {
        }
    }
}

#endif