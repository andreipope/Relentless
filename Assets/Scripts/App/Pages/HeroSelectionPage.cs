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
        private IDataManager _dataManager;

        private GameObject _selfPage;

        private Button _buttonBack,
                                _buttonContinue;
        private Transform _heroesContainer;

        private Image _selectedHeroIcon,
                        _selectedHeroSkillIcon;

        //private Text _selectedHeroName;
        private int _currentHeroId;

		private Dictionary<Enumerators.SkillType, Sprite> _skillsIcons;
        private Dictionary<Enumerators.ElementType, Sprite> _heroIcons,
                                                            _selectedHeroIcons;

        public void Init()
        {
			_uiManager = GameClient.Get<IUIManager>();
			_loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
			_localizationManager = GameClient.Get<ILocalizationManager>();
			_dataManager = GameClient.Get<IDataManager>();

			_selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/HeroSelectionPage"));
			_selfPage.transform.SetParent(_uiManager.Canvas.transform, false);


			_buttonBack = _selfPage.transform.Find("Header/BackButton").GetComponent<Button>();
			_buttonContinue = _selfPage.transform.Find("ContinueButton/Button").GetComponent<Button>();

			_buttonBack.onClick.AddListener(BackButtonHandler);
			_buttonContinue.onClick.AddListener(ContinueButtonHandler);
             
			_selectedHeroIcon = _selfPage.transform.Find("SelectedHero/Mask/Icon").GetComponent<Image>();
            _selectedHeroSkillIcon = _selfPage.transform.Find("SelectedHero/SkillIcon").GetComponent<Image>();
            //_selectedHeroName = _selfPage.transform.Find("SelectedHero/Name/Text").GetComponent<Text>();

            _heroesContainer = _selfPage.transform.Find("HeroesContainer");

            _skillsIcons = new Dictionary<Enumerators.SkillType, Sprite>();
			_skillsIcons.Add(Enumerators.SkillType.FIRE_DAMAGE, _loadObjectsManager.GetObjectByPath<Sprite>("Images/hero_power_01"));
			_skillsIcons.Add(Enumerators.SkillType.HEAL, _loadObjectsManager.GetObjectByPath<Sprite>("Images/hero_power_02"));
			_skillsIcons.Add(Enumerators.SkillType.CARD_RETURN, _loadObjectsManager.GetObjectByPath<Sprite>("Images/hero_power_03"));
			_skillsIcons.Add(Enumerators.SkillType.FREEZE, _loadObjectsManager.GetObjectByPath<Sprite>("Images/hero_power_04"));
			_skillsIcons.Add(Enumerators.SkillType.TOXIC_DAMAGE, _loadObjectsManager.GetObjectByPath<Sprite>("Images/hero_power_05"));
			_skillsIcons.Add(Enumerators.SkillType.HEAL_ANY, _loadObjectsManager.GetObjectByPath<Sprite>("Images/hero_power_06"));

            _heroIcons = new Dictionary<Enumerators.ElementType, Sprite>();
            _heroIcons.Add(Enumerators.ElementType.AIR, _loadObjectsManager.GetObjectByPath<Sprite>("Images/SelectedHeroes/selecthero_air"));
            _heroIcons.Add(Enumerators.ElementType.EARTH, _loadObjectsManager.GetObjectByPath<Sprite>("Images/SelectedHeroes/selecthero_earth"));
            _heroIcons.Add(Enumerators.ElementType.FIRE, _loadObjectsManager.GetObjectByPath<Sprite>("Images/SelectedHeroes/selecthero_fire"));
            _heroIcons.Add(Enumerators.ElementType.LIFE, _loadObjectsManager.GetObjectByPath<Sprite>("Images/SelectedHeroes/selecthero_life"));
            _heroIcons.Add(Enumerators.ElementType.TOXIC, _loadObjectsManager.GetObjectByPath<Sprite>("Images/SelectedHeroes/selecthero_toxic"));
            _heroIcons.Add(Enumerators.ElementType.WATER, _loadObjectsManager.GetObjectByPath<Sprite>("Images/SelectedHeroes/selecthero_water"));

            _selectedHeroIcons = new Dictionary<Enumerators.ElementType, Sprite>();
            _selectedHeroIcons.Add(Enumerators.ElementType.AIR, _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/PanelSelection/panel_selectedhero_air"));
            _selectedHeroIcons.Add(Enumerators.ElementType.EARTH, _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/PanelSelection/panel_selectedhero_earth"));
            _selectedHeroIcons.Add(Enumerators.ElementType.FIRE, _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/PanelSelection/panel_selectedhero_fire"));
            _selectedHeroIcons.Add(Enumerators.ElementType.LIFE, _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/PanelSelection/panel_selectedhero_life"));
            _selectedHeroIcons.Add(Enumerators.ElementType.TOXIC, _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/PanelSelection/panel_selectedhero_toxic"));
            _selectedHeroIcons.Add(Enumerators.ElementType.WATER, _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/PanelSelection/panel_selectedhero_water"));

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
             for (int i = 0; i < Constants.HEROES_AMOUNT; i++)
			{
                Transform heroObject = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Elements/HeroItem")).transform;
                heroObject.SetParent(_heroesContainer, false);
				var icon = heroObject.Find("HeroIcon").GetComponent<Image>();
                

                if (i < _dataManager.CachedHeroesData.heroes.Count)
                {
					icon.sprite = _heroIcons[_dataManager.CachedHeroesData.heroes[i].element];
                    heroObject.Find("SelectedIcon").gameObject.SetActive(false);
                    heroObject.Find("LockedIcon").gameObject.SetActive(false);
                    heroObject.GetComponent<Button>().onClick.AddListener(() => { ChooseHeroHandler(heroObject); });
                    heroObject.Find("Name").GetComponent<Text>().text = _dataManager.CachedHeroesData.heroes[i].name;
                }
                else
                {
                    heroObject.Find("Name").gameObject.SetActive(false);
                    heroObject.Find("LockedIcon").gameObject.SetActive(true);
                    heroObject.Find("SelectedIcon").gameObject.SetActive(false);
                    icon.gameObject.SetActive(false);
                }
			}
            _currentHeroId = 0;
                SetActive(  _currentHeroId, true);
        }

		#region Buttons Handlers

		private void BackButtonHandler()
		{
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.DECK_SELECTION);
		}
        public void ContinueButtonHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
            (_uiManager.GetPage<DeckEditingPage>() as DeckEditingPage).CurrentHeroId = _currentHeroId;
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
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);
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
                _selectedHeroIcon.sprite = _selectedHeroIcons[_dataManager.CachedHeroesData.heroes[id].element];
                
                _selectedHeroSkillIcon.sprite = _skillsIcons[_dataManager.CachedHeroesData.heroes[id].skill.skillType];
                //_selectedHeroName.text = _dataManager.CachedHeroesData.heroes[id].name;
            }
		}
    }
}
                     