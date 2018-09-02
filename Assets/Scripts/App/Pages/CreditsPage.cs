using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class CreditsPage : IUIElement
    {
        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private GameObject _selfPage;

        private Button _buttonBack;

        private ButtonShiftingContent _buttonThanks;

        private GameObject _creditListItemPrefab, _creditSubsectionListItemPrefab;

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

            _creditListItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/CreditListItem");
            _creditSubsectionListItemPrefab = _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/CreditSubSectionListItem");
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
            _selfPage = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/CreditsPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _creditsListScroll = _selfPage.transform.Find("Panel_CreditsList").GetComponent<ScrollRect>();
            _panelCreditsList = _selfPage.transform.Find("Panel_CreditsList/Group");

            _buttonBack = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _buttonThanks = _selfPage.transform.Find("Panel_CreditsList/Group/ButtonSpace/Button_Thanks").GetComponent<ButtonShiftingContent>();

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
            for (int i = 0; i < _dataManager.CachedCreditsData.CreditsInfo.Count; i++)
            {
                if (i > 0)
                {
                    new CreditSubSectionView(_creditSubsectionListItemPrefab, _panelCreditsList, _dataManager.CachedCreditsData.CreditsInfo[i].SubsectionType);
                }

                for (int j = 0; j < _dataManager.CachedCreditsData.CreditsInfo[i].Credits.Count; j++)
                {
                    CreditView credit =
                        new CreditView(
                            _creditListItemPrefab,
                            _panelCreditsList,
                            _dataManager.CachedCreditsData.CreditsInfo[i].Credits[j].FullName,
                            _dataManager.CachedCreditsData.CreditsInfo[i].Credits[j].Post
                            );
                    _credits.Add(credit);
                }
            }

            _buttonThanks.transform.parent.SetAsLastSibling();
        }

        private void BackButtonOnClickHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Enumerators.AppState.MAIN_MENU);
        }
    }

    public class CreditView
    {
        public GameObject SelfObject;

        public TextMeshProUGUI FullNameText;

        public TextMeshProUGUI PostText;

        public CreditView()
        {
        }

        public CreditView(GameObject prefab, Transform parent, string name, string post)
        {
            SelfObject = Object.Instantiate(prefab, parent, false);
            FullNameText = SelfObject.transform.Find("Text_Name").GetComponent<TextMeshProUGUI>();
            PostText = SelfObject.transform.Find("Text_Post").GetComponent<TextMeshProUGUI>();
            FullNameText.text = name;
            if (string.IsNullOrWhiteSpace(name))
            {
                FullNameText.gameObject.SetActive(false);
            }

            PostText.text = post;
        }
    }

    public class CreditSubSectionView
    {
        public GameObject SelfObject;

        public TextMeshProUGUI SectionText;

        public CreditSubSectionView()
        {
        }

        public CreditSubSectionView(GameObject prefab, Transform parent, string section)
        {
            SelfObject = Object.Instantiate(prefab, parent, false);
            SectionText = SelfObject.transform.Find("Text_Section").GetComponent<TextMeshProUGUI>();
            SectionText.text = section;
        }
    }
}
