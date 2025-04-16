#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using EmergenceSDK.Runtime.Internal.Services;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using EmergenceSDK.Runtime.Types;
using UnityEditor;
using UnityEngine;

namespace EmergenceSDK.Tests.Internal.EditorWindowDrivenTests
{
    public class PersonaTesting : BaseTestWindow
    {
        private bool accessTokenRetrieved = false;
        private List<Persona> personas = new List<Persona>();
        private IPersonaService personaService;
        private IPersonaServiceInternal personaServiceInternal;
        private ISessionService sessionService;
        private ISessionServiceInternal sessionServiceInternal;
        
        private Persona currentPersona;
        private Persona testPersona;

        private void OnGUI()
        {
            if (!ReadyToTest(out var msg))
            {
                EditorGUILayout.LabelField(msg);
                return;
            }
            needsCleanUp = true;

            personaService ??= EmergenceServiceProvider.GetService<IPersonaService>();
            personaServiceInternal ??= EmergenceServiceProvider.GetService<IPersonaServiceInternal>();
            sessionService ??= EmergenceServiceProvider.GetService<ISessionService>();
            sessionServiceInternal ??= EmergenceServiceProvider.GetService<ISessionServiceInternal>();
            
            EditorGUILayout.LabelField("Test Persona Service");

            if (!accessTokenRetrieved)
            {
                if(GUILayout.Button("GetAccessToken")) 
                    GetAccessTokenPressed();
                
                return;
            }

            if (GUILayout.Button("GetPersona")) 
                GetPersonaPressed();

            foreach (var persona in personas)
            {
                EditorGUILayout.LabelField("Persona: " + persona.name);
                EditorGUILayout.LabelField("PersonaBio: " + persona.bio);
            }

            if(currentPersona == null)
                return;

            if (GUILayout.Button("Create Test Persona"))
            {
                Persona newPersona = new Persona();
                newPersona.name = "TestPersona";
                newPersona.bio = "TestBio";
                newPersona.avatar = currentPersona.avatar;
                personaServiceInternal.CreatePersona(newPersona, () => GetPersonaPressed(), EmergenceLogger.LogError);
            }

            if(testPersona == null)
                return;
            
            if (GUILayout.Button("Update Test Persona"))
            {
                testPersona.bio = "UpdatedBio";
                personaServiceInternal.EditPersona(testPersona, () => GetPersonaPressed(), EmergenceLogger.LogError);
            }
            
            if (GUILayout.Button("Delete Test Persona"))
            {
                personaServiceInternal.DeletePersona(testPersona, () => GetPersonaPressed(), EmergenceLogger.LogError);
            }
        }

        private void GetAccessTokenPressed()
        {
            sessionServiceInternal.GetAccessToken((accessToken) =>
            {
                accessTokenRetrieved = !String.IsNullOrEmpty(accessToken);
                Repaint();
            }, EmergenceLogger.LogError);
        }

        private void GetPersonaPressed()
        {
            personaServiceInternal.GetPersonas((personasIn, currentPersonaIn) =>
            {
                currentPersona = currentPersonaIn ?? personasIn.FirstOrDefault();
                personas = personasIn;
                TryStoreTestPersona();
                Repaint();
            }, EmergenceLogger.LogError);
        }

        private void TryStoreTestPersona()
        {
            testPersona = personas.FirstOrDefault(persona => persona.name == "TestPersona");
        }

        protected override void CleanUp()
        {
            personas.Clear();
            personaService = null;
            currentPersona = null;
            accessTokenRetrieved = false;
            testPersona = null;
        }
    }
}

#endif