using System;
using System.Collections.Generic;
using DG.Tweening;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class TutorialProgressInfoPopup : IUIPopup
    {
        private const float SpeedFilling = 0.35f;

        public event Action PopupHiding;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private ITutorialManager _tutorialManager;

        private IDataManager _dataManager;

        private TextMeshProUGUI _textProgress;

        private TextMeshProUGUI _textInfo;

        private TextMeshProUGUI _textTitle;

        private Image _imageProgressBar;

        public GameObject Self { get; private set; }

        private float _startValueProgressBar;

        private float _endValueProgressBar;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _dataManager = GameClient.Get<IDataManager>();

        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;
            
            Self.SetActive(false);
            UnityEngine.Object.Destroy(Self);
            Self = null;
            PopupHiding?.Invoke();
            PopupHiding = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            if (Self != null)
            {
                Hide();
            }

            Self = UnityEngine.Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/TutorialProgressInfoPopup"));

            Self.transform.SetParent(_uiManager.Canvas2.transform, false);

            _textProgress = Self.transform.Find("Text_Progress").GetComponent<TextMeshProUGUI>();
            _textInfo = Self.transform.Find("Text_Info").GetComponent<TextMeshProUGUI>();
            _textTitle = Self.transform.Find("Text_Title").GetComponent<TextMeshProUGUI>();

            _imageProgressBar = Self.transform.Find("Image_FillingProgressBar").GetComponent<Image>();

            int id = _tutorialManager.GetIndexOfCurrentTutorial();//CurrentTutorial.Id;

            SetTextProgress(id);

            _textInfo.text = string.Format("FINISH ALL {0} TUTORIALS TO START PVP", _tutorialManager.TutorialsCount.ToString());

            _textTitle.text = _tutorialManager.CurrentTutorial.Name;

            float step = 1f / _tutorialManager.TutorialsCount;
            _startValueProgressBar = step * id;
            _endValueProgressBar = step * (id + 1);
            _imageProgressBar.fillAmount = _startValueProgressBar;
            float duration = (_endValueProgressBar - _startValueProgressBar) / SpeedFilling;
            _imageProgressBar.DOFillAmount(_endValueProgressBar, duration).OnComplete(() =>
            {
                SetTextProgress(id + 1);
                InternalTools.DoActionDelayed(Hide, 1.5f);
            });
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
        }

        private void HideButtonOnClickHandler()
        {
            _uiManager.HidePopup<TutorialAvatarPopup>();

            _tutorialManager.ReportActivityAction(Enumerators.TutorialActivityAction.AvatarTooltipClosed);
        }

        private void SetTextProgress(int progress)
        {
            _textProgress.text = string.Format("{0}/{1}", progress, _tutorialManager.TutorialsCount);
        }
    }
}
