using System;
using System.Collections;
using System.Collections.Generic;
using EmergenceSDK.Runtime.Futureverse.Services;
using EmergenceSDK.Runtime.Services;
using Futureverse.FuturePass;
using Testbed.AssetRegister;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Custom editor to add label to inspector
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ExperienceController))]
public class ExperienceControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("This script controls the UX flow for this demo, including login and rendering procedures.", MessageType.Info);
        EditorGUILayout.Space();
        base.OnInspectorGUI();
    }
}
#endif

public class ExperienceController : MonoBehaviour
{
    [Header("General")]
    public GameObject backgroundFader;
        
    [Header("Login")]
    public FuturePassLoginManager loginManager; // Controls custodial authentication process
    public GameObject loginRoot; // Highest object in hierarchy that contains UI resources for login (aka first screen)
    public Button loginButton; // Starts custodial auth
    public Button walletButton; // Opens the input field to enter futurepass wallet
    public Button enterWalletButton; // Submits input field value as wallet
    public Button resetButton; // Restart the scene (useful for starting fresh without restarting app)
    public TMP_InputField enterWalletInputField; // Takes user input for wallet
    public TMP_Text loginText; // Displays login status (aka connecting text)

    [Header("Rendering")]
    public GameObject arRoot; // Player controller root (aka Armature)
    public GameObject arUI; // Root of grid UI used to render player assets
    public AssetRegisterExecutor arExecutor; // Futureverse tool for executing UBF graphs from an asset registry query
    
    private bool loggedIn;
    private string wallet;

    #region UI Event Subscriptions

        private void OnEnable()
        {
            loginButton.onClick.AddListener(OnLoginClicked);
            walletButton.onClick.AddListener(OnWalletClicked);
            enterWalletButton.onClick.AddListener(OnEnterWalletClicked);
            resetButton.onClick.AddListener(DoReset);
        }

        private void OnDisable()
        {
            loginButton.onClick.RemoveAllListeners();
            walletButton.onClick.RemoveAllListeners();
            enterWalletButton.onClick.RemoveAllListeners();
            resetButton.onClick.RemoveAllListeners();
        }

    #endregion
    
    private IEnumerator Start()
    {
        // Ensure that required objects are active, including buttons required to login
        backgroundFader.SetActive(true);
        loginRoot.SetActive(true);
        
        // Wait for futurepass or wallet login
        while (!loggedIn)
        {
            yield return null;
        }
        
        // Once logged in, disable login screen and enable rendering UI
        loginRoot.SetActive(false);
        backgroundFader.SetActive(false);
        arRoot.SetActive(true);
        arUI.SetActive(true);
        
        // Populate asset grid from wallet assets
        arExecutor.EnterWalletAndLoad(wallet);
    }

    private void OnLoginClicked()
    {
        loginText.text = "Connecting...\n";
            
        // Start custodial authentication process
        loginManager.Connect()
            .Forget();
        
        // If success, save wallet and flag state
        loginManager.onLoginSuccess?.AddListener((_) =>
        {
            loginText.text += "Logged in!";
            
            var fvService = EmergenceServiceProvider.GetService<IFutureverseService>();
            
            // Assume ROOT network, so discard network identifiers and just use wallet address
            var fp = fvService.CurrentFuturepassInformation.futurepass.Split(":")[^1]; 
            wallet = fp;
                
            loggedIn = true;
        });
            
        // If fail, inform user, but no other action required (they can just click login again)
        loginManager.onLoginFailed?.AddListener((errorContainer,_) =>
        {
            loginText.text += "Failed to login!\n";
            loginText.text += errorContainer.Exception.Message + '\n';
        });
    }

    // Open the wallet entry UI
    private void OnWalletClicked()
    {
        enterWalletInputField.gameObject.SetActive(true);
    }

    // Set wallet address from input field and flag state
    private void OnEnterWalletClicked()
    {
        wallet = enterWalletInputField.text;
        loggedIn = true;
    }

    // Restart scene for fresh state
    private void DoReset()
    {
        SceneManager.LoadScene(0);
    }
}
