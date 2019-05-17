using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Gameplay;
using UnityEngine;
using Object = UnityEngine.Object;
using System;

namespace Loom.ZombieBattleground
{
    public class YourTurnPopup : IUIPopup
    {
        private ILoadObjectsManager _loadObjectsManager;
        private ITutorialManager _tutorialManager;
        private IGameplayManager _gameplayManager;
        private IUIManager _uiManager;

        public Action OnPopupHide;

        public GameObject Self { get; private set; }

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();

            Hide();
        }

        public void Dispose()
        {
            GameClient.Get<ITimerManager>().StopTimer(HideDelay);
        }

        public void Hide()
        {
            GameClient.Get<ICameraManager>().FadeOut(null, 1);

            if (Self == null)
                return;

            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.EndTurn);

            OnPopupHide?.Invoke();

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YourTurnPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.YOURTURN_POPUP, Constants.SfxSoundVolume,
                false, false, true);
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            Self.transform.localScale = Vector3.zero;
            Self.transform.DOScale(1.0f, 0.4f).SetEase(Ease.InOutBack);
            GameClient.Get<ITimerManager>().AddTimer(HideDelay, null, Constants.DelayBetweenYourTurnPopup);
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
