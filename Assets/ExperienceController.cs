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

public class ExperienceController : MonoBehaviour
{
    public GameObject backgroundFader;
        
    public FuturePassLoginManager loginManager;
    public GameObject loginRoot;
    public Button loginButton;
    public Button walletButton;
    public Button enterWalletButton;
    public Button resetButton;
    public TMP_InputField enterWalletInputField;
    
    public TMP_Text loginText;

    public GameObject arRoot;
    public GameObject arUI;
    public AssetRegisterExecutor arExecutor;
    
    public TMP_Text errorText;
    
    private bool loggedIn = false;
    private string wallet;

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

    private IEnumerator Start()
    {
        backgroundFader.SetActive(true);
        loginRoot.SetActive(true);
        
        // Wait for futurepass or wallet login
        
        while (!loggedIn)
        {
            yield return null;
        }
        
        loginRoot.SetActive(false);
        backgroundFader.SetActive(false);
        arRoot.SetActive(true);
        arUI.SetActive(true);
        arExecutor.EnterWalletAndLoad(wallet);
    }

    void OnLoginClicked()
    {
        loginText.text = "Connecting...\n";
            
        loginManager.Connect()
            .Forget();
        
        loginManager.onLoginSuccess?.AddListener((_) =>
        {
            loginText.text += "Logged in!";
            
            var fvService = EmergenceServiceProvider.GetService<IFutureverseService>();
            var fp = fvService.CurrentFuturepassInformation.futurepass.Split(":")[^1];
            wallet = fp;
                
            loggedIn = true;
        });
            
        loginManager.onLoginFailed?.AddListener((errorContainer,_) =>
        {
            loginText.text += "Failed to login!\n";
            loginText.text += errorContainer.Exception.Message + '\n';
        });
    }

    void OnWalletClicked()
    {
        enterWalletInputField.gameObject.SetActive(true);
    }

    void OnEnterWalletClicked()
    {
        wallet = enterWalletInputField.text;
        loggedIn = true;
    }

    void DoReset()
    {
        SceneManager.LoadScene(0);
    }
}
