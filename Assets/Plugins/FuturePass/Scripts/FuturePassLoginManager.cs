// Placeholder Copyright Header

using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime;
using EmergenceSDK.Runtime.Futureverse.Services;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using UnityEngine;
using UnityEngine.Events;

namespace Futureverse.FuturePass
{
	public class FuturePassLoginManager : MonoBehaviour
	{
		[SerializeField] private LoginManager emergenceLoginManager;

		public bool connectOnStart;
		public bool autoRetryOnError;
		

		public UnityEvent<FuturePassError> Error = new();
		public UnityEvent Connected = new();

		public UnityEvent<string> onLoginSuccess = new();
		public UnityEvent<LoginExceptionContainer, bool> onLoginFailed = new();
		private void Start()
		{
			EmergenceServiceProvider.Load(ServiceProfile.Futureverse);

			emergenceLoginManager.loginStepUpdatedEvent.AddListener(OnLoginStepUpdated);
			emergenceLoginManager.loginFailedEvent.AddListener(OnLoginFailed);
			emergenceLoginManager.loginSuccessfulEvent.AddListener(OnLoginSuccess);
			
			if (connectOnStart)
			{
				Connect()
					.Forget();
			}
		}

		private void OnLoginSuccess(LoginManager manager, string result)
		{
			var walletService = EmergenceServiceProvider.GetService<IWalletService>();
			var futureService = EmergenceServiceProvider.GetService<IFutureverseService>();
			
			string final = walletService.WalletAddress + "\n\n" + result + "\n\n" + futureService.CurrentFuturepassInformation.futurepass;
			onLoginSuccess?.Invoke(final);
		}
		private void OnLoginFailed(LoginManager manager, LoginExceptionContainer exceptionContainer)
		{
			var code = FuturePassError.Code.UndefinedError;

			if (exceptionContainer.Exception is TimeoutException)
			{
				code = FuturePassError.Code.QrCodeError;
			}

			OnError(exceptionContainer.Exception.Message, code);
			onLoginFailed?.Invoke(exceptionContainer, autoRetryOnError);
		}

		private void OnLoginStepUpdated(LoginManager manager, LoginStep step, StepPhase phase)
		{
			if (phase != StepPhase.Success)
			{
				return;
			}

			if (step != LoginStep.CustodialRequests)
			{
				return;
			}
		}

		public async UniTaskVoid Connect()
		{
			await emergenceLoginManager.WaitUntilAvailable();

			await emergenceLoginManager.StartLogin(LoginSettings.EnableCustodialLogin);

			Connected.Invoke();
		}

		private void OnError(string message, FuturePassError.Code code)
		{
			if (autoRetryOnError)
			{
				Connect()
					.Forget();
			}

			Error.Invoke(
				new FuturePassError
				{
					message = message,
					code = code,
				}
			);
		}

		public class FuturePassError
		{
			public enum Code
			{
				UndefinedError,
				QrCodeError,
			}

			public Code code;

			public string message;
		}
	}
}