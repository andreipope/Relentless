﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GrandDevs.CZB.Common;
using GrandDevs.CZB.Gameplay;
using CCGKit;
using TMPro;


namespace GrandDevs.CZB
{
    public class HeroSelectionPage : IUIElement
    {
		private IUIManager _uiManager;
		private ILoadObjectsManager _loadObjectsManager;
		private ILocalizationManager _localizationManager;

        private GameObject _selfPage;

        private MenuButtonNoGlow _buttonBack,
                                _buttonContinue;
        private Transform _heroesContainer;

        private Image _selectedHeroIcon,
                        _selectedHeroSkillIcon;

        private Text _selectedHeroName;
        private int _currentHeroId;

        public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();

			_selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/HeroSelectionPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);


			_buttonBack = _selfPage.transform.Find("BackButton").GetComponent<MenuButtonNoGlow>();
			_buttonContinue = _selfPage.transform.Find("ContinueButton").GetComponent<MenuButtonNoGlow>();

			_buttonBack.onClickEvent.AddListener(BackButtonHandler);
			_buttonContinue.onClickEvent.AddListener(ContinueButtonHandler);
             
			_selectedHeroIcon = _selfPage.transform.Find("SelectedHero/Icon").GetComponent<Image>();
            _selectedHeroSkillIcon = _selfPage.transform.Find("SelectedHero/SkillIcon").GetComponent<Image>();
            _selectedHeroName = _selfPage.transform.Find("SelectedHero/Name/Text").GetComponent<Text>();

            _heroesContainer = _selfPage.transform.Find("HeroesContainer");
                          
            Hide();  
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage.SetActive(true);
            FillInfo();
        }

        public void Hide()
        {
            for (int i = 0; i < _heroesContainer.childCount; i++)
            {
                MonoBehaviour.Destroy(_heroesContainer.GetChild(i).gameObject);
            }
            _selfPage.SetActive(false);
        }

        public void Dispose()
        {
            
        }

        private void FillInfo()
        {
			for (uint i = 0; i < Constants.HEROES_AMOUNT; i++)
			{
                Transform heroObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/HeroItem")).transform;
                heroObject.SetParent(_heroesContainer, false);

                if (i < GameManager.Instance.heroes.Count)
                {
                    heroObject.Find("SelectedIcon").gameObject.SetActive(false);
                    heroObject.Find("LockedIcon").gameObject.SetActive(false);
                    heroObject.Find("Button").GetComponent<Button>().onClick.AddListener(() => { ChooseHeroHandler(heroObject); });
                }
                else
                {
                    heroObject.Find("NormalIcon").gameObject.SetActive(false);
                    heroObject.Find("SelectedIcon").gameObject.SetActive(false);
                }
			}
            _currentHeroId = 0;
                SetActive(  _currentHeroId, true);
        }

		#region Buttons Handlers

		private void BackButtonHandler()
		{
			GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.DECK_SELECTION);
		}
        public void ContinueButtonHandler()
        {
            GameManager.Instance.currentDeckId = -1;
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.DECK_EDITING);
           // OpenAlertDialog("You will have this chance soon ;) ");
        }

        private void ChooseHeroHandler(Transform deck)
        {
            int id = GetHeroId(deck);

            if (id == _currentHeroId)
                return;
            
			if (  _currentHeroId > -1)
				SetActive(  _currentHeroId, false);
			  _currentHeroId = id;
            SetActive(id, true);
        }
		
        private int GetHeroId(Transform hero)
        {
            int id = -1;
			for (int i = 0; i < _heroesContainer.childCount; i++)
			{
                if (_heroesContainer.GetChild(i) == hero)
                {
                    id = i;
                    break;
                }
			}
            return id;
        }
		#endregion

		private void OpenAlertDialog(string msg)
		{
			_uiManager.DrawPopup<WarningPopup>(msg);
		}

		public void SetActive(int id, bool active)
		{
            Transform activatedHero = _heroesContainer.GetChild(id);
            Transform selectedIcon = activatedHero.Find("SelectedIcon");
            selectedIcon.gameObject.SetActive(active);

            if(active)
            {
                //_selectedHeroIcon.sprite = selectedIcon.GetComponent<Image>().sprite;
                //_selectedHeroSkillIcon = _selfPage.transform.Find("SelectedHero/SkillIcon").GetComponent<Image>();
                _selectedHeroName.text = GameManager.Instance.heroes[id].name;
            }
		}
    }
}
