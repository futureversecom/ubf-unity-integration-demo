// Placeholder Copyright Header

using System;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime;
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

		[Header("Events")]
		public UnityEvent<Texture2D> PresentQrCode = new();

		public UnityEvent<FuturePassError> Error = new();
		public UnityEvent Connected = new();

		/*#if UNITY_EDITOR
				[Header("Editor Testing")]
				public bool simulateConnection;

				public string simualtedWalletAddress;
		#endif*/

		private void Start()
		{
			EmergenceServiceProvider.Load(ServiceProfile.Futureverse);

			emergenceLoginManager.loginStepUpdatedEvent.AddListener(OnLoginStepUpdated);
			emergenceLoginManager.loginFailedEvent.AddListener(OnLoginFailed);

			if (connectOnStart)
			{
				Connect()
					.Forget();
			}
		}

		private void OnLoginFailed(LoginManager manager, LoginExceptionContainer exceptionContainer)
		{
			var code = FuturePassError.Code.UndefinedError;

			if (exceptionContainer.Exception is TimeoutException)
			{
				code = FuturePassError.Code.QrCodeError;
			}

			OnError(exceptionContainer.Exception.Message, code);
		}

		private void OnLoginStepUpdated(LoginManager manager, LoginStep step, StepPhase phase)
		{
			if (phase != StepPhase.Success)
			{
				return;
			}

			if (step != LoginStep.QrCodeRequest)
			{
				return;
			}

			PresentQrCode.Invoke(emergenceLoginManager.CurrentQrCode.Texture);
		}

		public async UniTaskVoid Connect()
		{
			//TODO: optionally connect with spoofed connection

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