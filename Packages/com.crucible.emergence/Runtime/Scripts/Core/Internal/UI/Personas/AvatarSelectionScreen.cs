using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EmergenceSDK.Runtime.Internal.Utils;
using EmergenceSDK.Runtime.Services;
using UnityEngine;
using UnityEngine.UI;
using Avatar = EmergenceSDK.Runtime.Types.Avatar;

namespace EmergenceSDK.Runtime.Internal.UI
{
    public class AvatarSelectionScreen : MonoBehaviour
    {
        public Button.ButtonClickedEvent ReplaceAvatarClicked => ReplaceAvatarButton.onClick;
        
        public Transform AvatarScrollRoot;
        public Pool AvatarScrollItemsPool;
        
        [SerializeField]
        private Button ReplaceAvatarButton;
        [SerializeField]
        private RawImage PersonaAvatarBackground;
        [SerializeField]
        private RawImage PersonaAvatar;
        public Texture2D DefaultImage;
        
        private HashSet<string> imagesRefreshing = new HashSet<string>();
        private IAvatarService AvatarService => EmergenceServiceProvider.GetService<IAvatarService>();
        private bool requestingInProgress = false;
        
        public Avatar CurrentAvatar = null;
        
        private void Awake()
        {
            AvatarScrollItem.OnAvatarSelected += OnAvatarSelected;
            AvatarScrollItem.OnImageCompleted += OnImageCompleted;
            
            ReplaceAvatarButton.onClick.AddListener(OnReplaceClicked);
        }

        private void OnReplaceClicked()
        {
            SetButtonActive(false);
            AvatarScrollRoot.gameObject.SetActive(true);
        }

        private void OnDestroy()
        {
            AvatarScrollItem.OnAvatarSelected -= OnAvatarSelected;
            AvatarScrollItem.OnImageCompleted -= OnImageCompleted;
        }

        public void SetAvatarTexture(Texture2D texture)
        {
            PersonaAvatar.texture = texture;
            PersonaAvatarBackground.texture = texture;
        }
        
        public void SetButtonActive(bool active) => ReplaceAvatarButton.gameObject.SetActive(active);

        public void RefreshAvatarDisplay(Texture2D personaImage)
        {
            SetAvatarTexture(personaImage ? personaImage : DefaultImage);

            // Clear scroll area
            while (AvatarScrollRoot.childCount > 0)
            {
                GameObject child = AvatarScrollRoot.GetChild(0).gameObject;
                AvatarScrollItemsPool.ReturnUsedObject(child);
            }

            var cts = new CancellationTokenSource();
            ModalCancel.Instance.Show("Retrieving avatar data...", () => {
                cts.Cancel();
            });

            // Default avatar
            GameObject go = AvatarScrollItemsPool.GetNewObject();
            go.transform.SetParent(AvatarScrollRoot);
            go.transform.localScale = Vector3.one;

            go.GetComponent<AvatarScrollItem>().Refresh(DefaultImage, null);
            if (!cts.Token.IsCancellationRequested)
            {
                AvatarService.AvatarsByOwner(EmergenceServiceProvider.GetService<IWalletService>().WalletAddress, (avatars) =>
                    {
                        ModalCancel.Instance.Show("Retrieving avatar images...", () =>
                        {
                            cts.Cancel();
                        });
                        
                        requestingInProgress = true;
                        imagesRefreshing.Clear();
                        for (int i = 0; i < avatars.Count; i++)
                        {
                            cts.Token.ThrowIfCancellationRequested();
                            go = AvatarScrollItemsPool.GetNewObject();
                            go.transform.SetParent(AvatarScrollRoot);
                            go.transform.localScale = Vector3.one;

                            imagesRefreshing.Add(avatars[i].avatarId);
                            go.GetComponent<AvatarScrollItem>().Refresh(DefaultImage, avatars[i]);
                        }
                        requestingInProgress = false;
                        if (imagesRefreshing.Count <= 0)
                        {
                            ModalCancel.Instance.Hide();
                        }
                    },
                    (error, code) =>
                    {
                        EmergenceLogger.LogError(error, code);
                        ModalCancel.Instance.Hide();
                    },
                    () =>
                    {
                        requestingInProgress = false;
                        ModalCancel.Instance.Hide();
                        ScreenManager.Instance.ShowDashboard();
                    },
                    ct: cts.Token);
            }
        }
        
        private void OnImageCompleted(Avatar avatar, bool success)
        {
            if (!success)
            {
                HandleImageFailure(avatar);
            }
            else
            {
                HandleImageSuccess(avatar);
            }
        }

        private void HandleImageFailure(Avatar avatar)
        {
            if (avatar != null && imagesRefreshing.Contains(avatar.avatarId))
            {
                RemoveAndCheckModal(avatar.avatarId);
            }
        }

        private void HandleImageSuccess(Avatar avatar)
        {
            if (imagesRefreshing.Contains(avatar.avatarId))
            {
                RemoveAndCheckModal(avatar.avatarId);
            }
            else if (imagesRefreshing.Count > 0)
            {
                EmergenceLogger.LogWarning($"Image completed but not accounted for: [{avatar.avatarId}][{avatar.meta.content.First().url}][true]");
            }
        }

        private void RemoveAndCheckModal(string avatarId)
        {
            imagesRefreshing.Remove(avatarId);
            if (imagesRefreshing.Count <= 1 && !requestingInProgress)
            {
                ModalCancel.Instance.Hide();
            }
        }

        public void OnAvatarSelected(Avatar avatar)
        {
            CurrentAvatar = avatar;

            if (CurrentAvatar == null)
            {
                SetAvatarTexture(DefaultImage);
                return;
            }

            RequestImage.Instance.AskForImage(avatar.meta.content.First().url, 
                (url, texture) => SetAvatarTexture(texture), 
                EmergenceLogger.LogError);
        }
    }
}