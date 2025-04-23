using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EmergenceSDK.Runtime.Internal.Services;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using UnityEngine;

namespace EmergenceSDK.Runtime.Internal.UI
{
    public class PersonaUIManager
    {
        public static PersonaUIManager Instance;

        private readonly DashboardScreen dashboardScreen;
        private readonly Pool personaButtonPool;
        private readonly PersonaCarousel personaCarousel;
        private readonly Transform personaScrollContents;
        private IPersonaServiceInternal personaServiceInternal;
        internal readonly PersonaScrollItemStore PersonaScrollItemStore = new();

        private HashSet<string> imagesRefreshing = new();
        
        private Persona SelectedPersona => PersonaCarousel.Instance.SelectedPersona;
        private Persona ActivePersona => PersonaScrollItemStore.GetCurrentPersonaScrollItem()?.Persona;
        
        private bool requestingInProgress = false;
        
        private Texture2D defaultAvatarThumbnail;
        private Texture2D DefaultAvatarThumbnail =>  defaultAvatarThumbnail ??= Resources.Load<Texture2D>("DefaultAvatarThumbnail")  ?? new Texture2D(100, 100);

        public PersonaUIManager(DashboardScreen dashboardScreen, Pool personaButtonPool,
            PersonaCarousel personaCarousel, Transform personaScrollContents)
        {
            this.dashboardScreen = dashboardScreen;
            this.personaButtonPool = personaButtonPool;
            this.personaCarousel = personaCarousel;
            personaCarousel.Items = PersonaScrollItemStore;
            this.personaScrollContents = personaScrollContents;

            dashboardScreen.CreatePersonaClicked += OnCreatePersona;
            dashboardScreen.ActivePersonaClicked += OnActivePersonaClicked;
            dashboardScreen.UsePersonaAsCurrentClicked += OnUsePersonaAsCurrent;
            dashboardScreen.EditPersonaClicked += OnEditPersona;
            dashboardScreen.DeletePersonaClicked += OnDeletePersona;

            PersonaScrollItem.OnSelected += PersonaScrollItem_OnSelected;
            PersonaScrollItem.OnImageCompleted += PersonaScrollItem_OnImageCompleted;
            
            personaCarousel.ArrowClicked += PersonaCarousel_OnArrowClicked;

            Instance = this;
        }
        
        
        private void OnCreatePersona()
        {
            ScreenManager.Instance.ShowEditPersona();
            EditPersonaScreen.Instance.OnCreatePersonaClicked();
        }
        
        private void OnActivePersonaClicked()
        {
            if (ActivePersona == null)
            {
                return;
            }
            PersonaScrollItem_OnSelected(ActivePersona, -1);
        }
        
        private async void OnUsePersonaAsCurrent()
        {
            Modal.Instance.Show("Loading Personas...");
            var setPersonaResponse = await personaServiceInternal.SetCurrentPersonaAsync(SelectedPersona);
            if (setPersonaResponse.Successful)
            {
                Refresh().Forget();
            }
            Modal.Instance.Hide();
            if(!setPersonaResponse.Successful)
            {
                ModalPromptOK.Instance.Show("Error setting current persona");
            }
        }
        
        private void OnEditPersona()
        {
            ScreenManager.Instance.ShowEditPersona();
            EditPersonaScreen.Instance.OnEditPersonaClicked(SelectedPersona, ActivePersona.id == SelectedPersona.id);
        }

        private void OnDeletePersona() => ModalPromptYESNO.Instance.Show($"Deleting persona: {SelectedPersona.name}", "Are you sure?", DeleteSelectedPersona);

        private async void DeleteSelectedPersona()
        {
            Modal.Instance.Show("Deleting Persona...");
            var personaServiceResult = await personaServiceInternal.DeletePersonaAsync(SelectedPersona);
            if (personaServiceResult.Successful)
            {
                Refresh().Forget();
            }
            Modal.Instance.Hide();
        }

        public async UniTask Refresh()
        {
            personaServiceInternal = EmergenceServiceProvider.GetService<IPersonaServiceInternal>();
            
            dashboardScreen.HideUI();
            dashboardScreen.HideDetailsPanel();
            while (personaScrollContents.childCount > 0)
            {
                personaButtonPool.ReturnUsedObject(personaScrollContents.GetChild(0).gameObject);
            }

            Modal.Instance.Show("Loading Personas...");
            
            var personas = await personaServiceInternal.GetPersonasAsync();
            if (personas.Successful)
            {
                if(personas.Result1.Count == 0 || personas.Result2 == null)
                {
                    Modal.Instance.Hide();
                    dashboardScreen.ShowUI(true);
                    return;
                }
                GetPersonasSuccess(personas.Result1);
            }
            else
            {
                ModalPromptOK.Instance.Show("Error retrieving personas");
            }
            personaCarousel.GoToActivePersona();
        }

        private void GetPersonasSuccess(List<Persona> personas)
        {
            Modal.Instance.Show("Retrieving avatars...");
            try
            {
                requestingInProgress = true;
                imagesRefreshing.Clear();

                //Update scroll items
                List<PersonaScrollItem> scrollItems = CreateScrollItems(personas);
                if(scrollItems.Count > 0)
                {
                    PersonaScrollItemStore.SetPersonas(scrollItems);
                    RefreshScrollItems(personas, scrollItems);
                }

                PersonaCarousel.Instance.Refresh();
                dashboardScreen.ShowUI(PersonaScrollItemStore.Count == 0);
                PersonaScrollItem_OnSelected(ActivePersona, PersonaScrollItemStore.GetCurrentPersonaIndex());
                requestingInProgress = false;
            }
            catch (Exception e)
            {
                EmergenceLogger.LogError(e.Message);
            }
            finally
            {
                Modal.Instance.Hide();
            }
        }
        
        private List<PersonaScrollItem> CreateScrollItems(List<Persona> personas)
        {
            List<PersonaScrollItem> scrollItems = new List<PersonaScrollItem>();

            for (int i = 0; i < personas.Count; i++)
            {
                // Creation and setup of scroll item
                GameObject go = personaButtonPool.GetNewObject();
                go.transform.SetParent(personaScrollContents);
                go.transform.localScale = Vector3.one;

                scrollItems.Add(personaScrollContents.GetChild(i).GetComponent<PersonaScrollItem>());

                Persona persona = personas[i];

                // Avatar validation logic
                if (persona.avatar != null)
                {
                    if (string.IsNullOrEmpty(persona.avatar.avatarId) || string.IsNullOrEmpty(persona.avatar.meta.content.First().url))
                    {
                        persona.avatar = null;
                    }
                }
            }

            return scrollItems;
        }
        
        private void RefreshScrollItems(List<Persona> personas, List<PersonaScrollItem> scrollItems)
        {
            for (int i = 0; i < personas.Count; i++)
            {
                Persona persona = personas[i];
                imagesRefreshing.Add(persona.id);
                scrollItems[i].Refresh(DefaultAvatarThumbnail, persona);
            }
        }
        
        
        private void PersonaCarousel_OnArrowClicked(Persona persona)
        {
            OnNewItemSelected(persona);
        }

        private void PersonaScrollItem_OnSelected(Persona persona, int position)
        {
            if (OnNewItemSelected(persona)) 
                return;
            personaCarousel.GoToPosition(persona);
        }

        private bool OnNewItemSelected(Persona persona)
        {
            if (persona == null)
            {
                return true;
            }

            bool active = ActivePersona != null && persona.id == ActivePersona.id;

            dashboardScreen.ShowDetailsPanel(persona, active);
            return false;
        }

        private void PersonaScrollItem_OnImageCompleted(Persona persona, bool success)
        {
            if (ActivePersona != null)
            {
                if (ActivePersona.id.Equals(persona.id))
                {
                     dashboardScreen.ShowAvatar(persona.AvatarImage);
                }
            }

            if (imagesRefreshing.Contains(persona.id))
            {
                imagesRefreshing.Remove(persona.id);
                if (imagesRefreshing.Count <= 0 && !requestingInProgress)
                {
                    Modal.Instance.Hide();
                }
            }
        }
    }
}