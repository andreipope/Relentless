using System;
using DG.Tweening;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class YourTurnPopup : IUIPopup
    {
        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            Hide();
        }

        public void Dispose()
        {
            GameClient.Get<ITimerManager>().StopTimer(HideDelay);
        }

        public void Hide()
        {
            OnHidePopupEvent?.Invoke();
            GameClient.Get<ICameraManager>().FadeOut(null, 1);

            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YourTurnPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.YourturnPopup, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            Self.transform.localScale = Vector3.zero;
            Self.transform.DOScale(1.0f, 0.4f).SetEase(Ease.InOutBack);
            GameClient.Get<ITimerManager>().AddTimer(HideDelay, null, 4f, false);
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        private void HideDelay(object[] param)
        {
            Hide();
        }
    }
}
