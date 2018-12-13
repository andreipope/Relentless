using System.Collections.Generic;
using System.IO;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class CreditsPage : IUIElement
    {
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private GameObject _selfPage;

        private Button _buttonBack;

        private ButtonShiftingContent _buttonThanks;

        private GameObject _creditListItemPrefab, _creditPostListItemPrefab, _creditSubsectionListItemPrefab;

        private ScrollRect _creditsListScroll;

        private List<CreditView> _credits;

        private Transform _panelCreditsList;

        private bool _isActive;

        public void Init()
        {
            _credits = new List<CreditView>();
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();

            _creditPostListItemPrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/PostListItem");
            _creditListItemPrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/CreditListItem");
            _creditSubsectionListItemPrefab =
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/CreditSubSectionListItem");
        }

        public void Update()
        {
            if (_isActive)
            {
                _creditsListScroll.verticalNormalizedPosition -= Time.deltaTime / 70;
            }
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/CreditsPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _creditsListScroll = _selfPage.transform.Find("Panel_CreditsList").GetComponent<ScrollRect>();
            _panelCreditsList = _selfPage.transform.Find("Panel_CreditsList/Group");

            _buttonBack = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _buttonThanks = _selfPage.transform.Find("Panel_CreditsList/Group/ButtonSpace/Button_Thanks")
                .GetComponent<ButtonShiftingContent>();

            _buttonBack.onClick.AddListener(BackButtonOnClickHandler);
            _buttonThanks.onClick.AddListener(BackButtonOnClickHandler);

            _isActive = true;
            _creditsListScroll.verticalNormalizedPosition = 1;

            FillCredits();
        }

        public void Hide()
        {
            _isActive = false;

            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
        }

        public void Dispose()
        {
        }

        private void FillCredits()
        {
            CreditView post = null;
            for (int i = 0; i < _dataManager.CachedCreditsData.CreditsInfo.Count; i++)
            {
                new CreditSubSectionView(_creditSubsectionListItemPrefab, _panelCreditsList,
                    _dataManager.CachedCreditsData.CreditsInfo[i].SubsectionType);
                for (int j = 0; j < _dataManager.CachedCreditsData.CreditsInfo[i].Posts.Count; j++)
                {
                        post =
                        new CreditView(
                            _creditPostListItemPrefab,
                            _panelCreditsList,
                            _dataManager.CachedCreditsData.CreditsInfo[i].Posts[j].Post
                        );

                    for (int k = 0; k < _dataManager.CachedCreditsData.CreditsInfo[i].Posts[j].Credits.Count; k++)
                    {
                        CreditView credit =
                        new CreditView(
                            _creditListItemPrefab,
                            post.SelfObject.transform,
                            _dataManager.CachedCreditsData.CreditsInfo[i].Posts[j].Credits[k].FullName
                        );
                        _credits.Add(credit);
                    }
                }
            }

            _buttonThanks.transform.parent.SetAsLastSibling();
        }

        private void BackButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }
    }

    public class CreditView
    {
        public GameObject SelfObject;

        public TextMeshProUGUI FullNameText;

        public CreditView(GameObject prefab, Transform parent, string name)
        {
            SelfObject = Object.Instantiate(prefab, parent, false);
            FullNameText = SelfObject.transform.Find("Text_Name").GetComponent<TextMeshProUGUI>();
            FullNameText.text = name;
            if (string.IsNullOrWhiteSpace(name))
            {
                FullNameText.gameObject.SetActive(false);
            }
        }
    }

    public class CreditSubSectionView
    {
        public GameObject SelfObject;

        public TextMeshProUGUI SectionText;

        public CreditSubSectionView(GameObject prefab, Transform parent, string section)
        {
            SelfObject = Object.Instantiate(prefab, parent, false);
            SectionText = SelfObject.transform.Find("Text_Section").GetComponent<TextMeshProUGUI>();
            SectionText.text = section;
        }
    }
}
