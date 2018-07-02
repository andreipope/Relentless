using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using UnityEngine.Networking;
using GrandDevs.CZB.Data;
using GrandDevs.Internal;
using DG.Tweening;
using GrandDevs.CZB.Gameplay;

namespace GrandDevs.CZB
{
    public class YourTurnPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private GameObject _selfPage;


        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YourTurnPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas3.transform, false);

            Hide();
        }


        public void Dispose()
        {
            GameClient.Get<ITimerManager>().StopTimer(HideDelay);
        }

        public void Hide()
        {
            OnHidePopupEvent?.Invoke();
            _selfPage.SetActive(false);
			GameClient.Get<ICameraManager>().FadeOut(null, 1);

		}

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.YOURTURN_POPUP, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<ICameraManager>().FadeIn(0.7f, 1);
            _selfPage.SetActive(true);

            _selfPage.transform.localScale = Vector3.zero;
            _selfPage.transform.DOScale(1.0f, 0.4f).SetEase(Ease.InOutBack);
            GameClient.Get<ITimerManager>().AddTimer(HideDelay, null, 2f, false);
        }

        public void Show(object data)
        {
            Show();

        }

        private void HideDelay(object[] param)
        {
            var sequence = DOTween.Sequence();
            sequence.Append(_selfPage.transform.DOScale(0.0f, 0.2f).SetEase(Ease.OutCubic));
            sequence.OnComplete(() => 
            {
                Hide();
            });
        }

        public void Update()
        {

        }

        private void OnClickOkButtonEventHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            if (GameClient.Get<ITutorialManager>().IsTutorial)
                GameClient.Get<ITutorialManager>().StopTutorial();

            GameClient.Get<IAppStateManager>().ChangeAppState(GrandDevs.CZB.Common.Enumerators.AppState.DECK_SELECTION);
            Hide();
        }

    }
}