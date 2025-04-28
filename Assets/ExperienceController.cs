using System;
using System.Collections;
using System.Collections.Generic;
using EmergenceSDK.Runtime.Futureverse.Services;
using EmergenceSDK.Runtime.Services;
using Futureverse.FuturePass;
using Testbed.AssetRegister;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceController : MonoBehaviour
{
    public GameObject backgroundFader;
        
    public FuturePassLoginManager loginManager;
    public GameObject loginRoot;
    public Button loginButton;
    public TMP_Text loginText;

    public GameObject arRoot;
    public GameObject arUI;
    public AssetRegisterExecutor arExecutor;
    
    public TMP_Text errorText;
    
    private bool loggedIn = false;
    private IEnumerator Start()
    {
        backgroundFader.SetActive(true);
        loginRoot.SetActive(true);

        while (!loggedIn)
        {
            bool btnClicked = false;
            loginButton.onClick.AddListener(() => btnClicked = true);
            yield return new WaitUntil(() => btnClicked);

            loginText.text = "Connecting...\n";
            
            loginManager.Connect()
                .Forget();

            bool loginEvent = false;
            bool loginSuccess = false;
            
            loginManager.onLoginSuccess?.AddListener((_) =>
            {
                loginEvent = true;
                loginSuccess = true;
                loginText.text += "Logged in!";
            });
            
            loginManager.onLoginFailed?.AddListener((errorContainer,_) =>
            {
                loginEvent = true;
                loginSuccess = false;
                loginText.text += "Failed to login!\n";
                loginText.text += errorContainer.Exception.Message + '\n';
            });
            
            while (!loginEvent)
            {
                yield return null;
            }

            if (loginSuccess)
            {
                loggedIn = true;
            }
        }
        loginRoot.SetActive(false);
        backgroundFader.SetActive(false);
        arRoot.SetActive(true);
        arUI.SetActive(true);
        
        var fvService = EmergenceServiceProvider.GetService<IFutureverseService>();
        var fp = fvService.CurrentFuturepassInformation.futurepass.Split(":")[^1];
        arExecutor.EnterWalletAndLoad(fp);
    }
}
