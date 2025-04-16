using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace EmergenceSDK.Samples.CoreSamples.DemoStations
{
    public class OpenOverlay : DemoStation<OpenOverlay>, IDemoStation
    {
        private IPersonaService personaService;
        private IAvatarService avatarService;

        public bool IsReady
        {
            get => isReady;
            set
            {
                InstructionsText.text = value ? ActiveInstructions : InactiveInstructions;
                isReady = value;
            }
        }

        private void Start()
        {
            EmergenceServiceProvider.OnServicesLoaded += _ =>
            {
                personaService = EmergenceServiceProvider.GetService<IPersonaService>();
                personaService.OnCurrentPersonaUpdated += OnPersonaUpdated;
                avatarService = EmergenceServiceProvider.GetService<IAvatarService>();
            };
            instructionsGO.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            instructionsGO.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            instructionsGO.SetActive(false);
        }

        private void Update()
        {
            if (HasBeenActivated())
            {
                EmergenceSingleton.Instance.OpenEmergenceUI();
            }
        }

        public void OnPersonaUpdated(Persona persona) 
        {
            EmergenceLogger.LogInfo("Changing avatar", true);
            if (persona != null && !string.IsNullOrEmpty(persona.avatarId))
            {
                
                avatarService.AvatarById(persona.avatarId, (async avatar =>
                {
                    var request = UnityWebRequest.Get(Helpers.InternalIPFSURLToHTTP(avatar.tokenURI));
                    string response;
                    using (request.uploadHandler)
                    {
                        await request.SendWebRequest().ToUniTask();
                        response = request.downloadHandler.text;
                    }

                    try
                    {
                        var token = SerializationHelper.Deserialize<EASMetadata[]>(response);
                        SimpleAvatarSwapper.Instance.SwapAvatars(GameObject.Find("PlayerArmature"),
                            Helpers.InternalIPFSURLToHTTP(token[0].UriBase)).Forget();
                    }
                    catch (JsonException) {}
                }), EmergenceLogger.LogError);
            }
            else
            {
                SimpleAvatarSwapper.Instance.SetDefaultAvatar(GameObject.Find("PlayerArmature"));
            }
        }
    }
}