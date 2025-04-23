using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.ScriptableObjects;
using EmergenceSDK.Runtime.Services;
using UnityEngine;

namespace EmergenceSDK.Samples.CoreSamples.Examples
{
    public class ReadingContracts : MonoBehaviour
    {
        [Header("Contract information")]
        // This must be set in the inspector
        public DeployedSmartContract deployedContract;
        // Public string array that is used as input data for the smart contract method
        public string[] body = new string[] { };

        private IContractService contractService;
        
        public void Awake()
        {
            contractService = EmergenceServiceProvider.GetService<IContractService>();
        }

        public void Start()
        {
            ReadContract();
        }

        private void ReadContract()
        {
            // Creates a ContractInfo object with the smart contract address, method name, network name, and default node URL
            var contractInfo = new ContractInfo(deployedContract, "[METHOD NAME]");

            // Calls the ReadMethod method to execute the smart contract method defined in the ABI with an empty input parameter
            contractService.ReadMethod(contractInfo, body, ReadSuccess, EmergenceLogger.LogError);
        }

        // This method is called when the ReadMethod method executes successfully
        private void ReadSuccess<T>(T response)
        {
            // Logs the response to the console
            EmergenceLogger.LogInfo($"{response}");
        }
    }
}
