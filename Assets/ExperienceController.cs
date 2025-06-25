using System;
using System.Collections;
using System.Collections.Generic;
using AssetRegister.Runtime.Clients;
using AssetRegister.Runtime.Schema.Objects;
using Futureverse.UBF.UBFExecutionController.Runtime;
using Futureverse.UBF.UBFExecutionController.Samples;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Auth = Futureverse.FuturePass.FuturePassAuthentication;

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
    public enum Environment // A single environment variable to tie together the various sdks
    {
        Staging,
        Production
    }

    [Header("General")] 
    public Environment authEnvironment;
    public GameObject backgroundFader;
        
    [Header("Login")]
    public GameObject loginRoot; // Highest object in hierarchy that contains UI resources for login (aka first screen)
    public Button loginButton; // Starts custodial auth
    public Button walletButton; // Opens the input field to enter futurepass wallet
    public Button enterWalletButton; // Submits input field value as wallet
    public Button resetButton; // Restart the scene (useful for starting fresh without restarting app)
    public TMP_InputField enterWalletInputField; // Takes user input for wallet
    public TMP_Text loginText; // Displays login status (aka connecting text)

    [Header("Assets")]
    public AssetUI assetUI;
    public RectTransform assetsGrid;

    [Header("Rendering")]
    public GameObject arRoot; // Player controller root (aka Armature)
    public GameObject arUI; // Root of grid UI used to render player assets
    public ExecutionController executionController;
    
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
        Auth.SetEnvironment(authEnvironment == Environment.Staging ? Auth.Environment.Staging : Auth.Environment.Production);
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
        enterWalletInputField.text = wallet;
        StartCoroutine(executionController.FetchAssetsFromWallet(wallet, OnAssetsLoaded, OnFailure));
    }
    
    private void OnFailure(string error)
    {
        // TODO: Set some error text?
    }

    private void OnAssetsLoaded(Asset[] assets)
    {
        foreach (var asset in assets)
        {
            var ui = Instantiate(assetUI, assetsGrid);
            ui.Load(asset, () => LoadAsset(asset));
        }
    }
    
    private void LoadAsset(Asset asset)
    {
        StartCoroutine(executionController.LoadUBFAsset(asset));
    }

    private void OnLoginClicked()
    {
        loginText.text = "Connecting...\n";
            
        // Start custodial authentication process
        Auth.StartLogin(() =>
        {
            loginText.text += "Logged in!";
            wallet = Auth.LoadedAuthenticationDetails.DecodedToken.Futurepass;
            Debug.Log("Logged in with wallet: " + wallet);
            loggedIn = true;
        }, exception =>
        {
            loginText.text += "Failed to login!\n";
            loginText.text += exception.Message + '\n';
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
