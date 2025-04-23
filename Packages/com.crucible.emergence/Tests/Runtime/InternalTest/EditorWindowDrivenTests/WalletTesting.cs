#if UNITY_EDITOR

using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK.Tests.Internal.EditorWindowDrivenTests
{
    public class WalletTesting : BaseTestWindow
    {
        private IWalletService walletService;

        private string walletBalance = "";
        
        private void OnGUI()
        {
            if (!ReadyToTest(out var msg))
            {
                EditorGUILayout.LabelField(msg);
                return;
            }
            needsCleanUp = true;
            
            walletService ??= EmergenceServiceProvider.GetService<IWalletService>();
            
            EditorGUILayout.LabelField("Test Wallet Service");
            if(GUILayout.Button("Test Sign"))
                TestSignPressed();

            if (GUILayout.Button("Test Validate Message"))
                TestValidateMessagePressed();
            
            if(GUILayout.Button("Get Balance"))
                GetBalancePressed();
            EditorGUILayout.LabelField(walletBalance);
        }

        private void TestValidateMessagePressed()
        {
            var messageToSign = "Test message";
            walletService.RequestToSign(messageToSign, message =>
            {
                walletService.ValidateSignedMessage(messageToSign, message, walletService.WalletAddress, isValid =>
                {
                    EmergenceLogger.LogInfo($"Message is valid: {isValid}");
                }, EmergenceLogger.LogError);
            }, EmergenceLogger.LogError);
        }

        private void GetBalancePressed()
        {
            walletService.GetBalance(balance =>
            {
                walletBalance = $"Wallet Balance: {balance}";
                EmergenceLogger.LogInfo($"Balance: {balance}");
            }, EmergenceLogger.LogError);
        }

        private void TestSignPressed()
        {
            walletService.RequestToSign("Test message", message => EmergenceLogger.LogInfo($"Signed: {message}"), EmergenceLogger.LogError);
        }
    }
}

#endif