using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using DG.Tweening;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class OverlordSelectionTab
    {
        private static readonly ILog Log = Logging.GetLog(nameof(OverlordSelectionTab));
        
        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;
        
        private ITutorialManager _tutorialManager;
        
        private IAnalyticsManager _analyticsManager;
        
        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;
        
        private HordeSelectionWithNavigationPage _myDeckPage;

        private GameObject _selfPage;

        private TextMeshProUGUI _textSelectOverlordDeckName,
                                _textSelectOverlordName,
                                _textSelectOverlordDescription;

        private Button _buttonSelectOverlordLeftArrow,
                       _buttonSelectOverlordRightArrow,
                       _buttonSelectOverlordContinue;

        private Image _imageSelectOverlordGlow,
                      _imageSelectOverlordPortrait,
                      _imageCross;
                       
        private List<Transform> _selectOverlordIconList;
                       
        private int _selectOverlordIndex;
        
        private const int NumberOfOverlord = 6;

        private Dictionary<Enumerators.Faction, Image> _elementImageDictionary;
        
        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            
            _selectOverlordIconList = new List<Transform>();
            
            _myDeckPage = GameClient.Get<IUIManager>().GetPage<HordeSelectionWithNavigationPage>();
            _myDeckPage.EventChangeTab += (HordeSelectionWithNavigationPage.Tab tab) =>
            {
                if (tab != HordeSelectionWithNavigationPage.Tab.SELECT_OVERLORD)
                    return;
                    
                _textSelectOverlordDeckName.text = "NEW DECK";

                int index = 0;

                if (_tutorialManager.IsTutorial)
                {
                    index = _dataManager.CachedHeroesData.Heroes.FindIndex(hero => hero.HeroElement == _tutorialManager.CurrentTutorial.TutorialContent.ToMenusContent().SpecificHordeInfo.MainSet);
                }

                ChangeOverlordIndex(index);
            };
            
            _elementImageDictionary = new Dictionary<Enumerators.Faction, Image>();
        }
        
        public void Show(GameObject selfPage)
        {
            _selfPage = selfPage;
            
            _textSelectOverlordDeckName = _selfPage.transform.Find("Tab_SelectOverlord/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();
           
            _buttonSelectOverlordLeftArrow = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Button_LeftArrow").GetComponent<Button>();
            _buttonSelectOverlordLeftArrow.onClick.AddListener(ButtonSelectOverlordLeftArrowHandler);
            
            _buttonSelectOverlordRightArrow = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Button_RightArrow").GetComponent<Button>();
            _buttonSelectOverlordRightArrow.onClick.AddListener(ButtonSelectOverlordRightArrowHandler);
            
            _buttonSelectOverlordContinue = _selfPage.transform.Find("Tab_SelectOverlord/Panel_FrameComponents/Lower_Items/Button_Continue").GetComponent<Button>();
            _buttonSelectOverlordContinue.onClick.AddListener(ButtonSelectOverlordContinueHandler);
            
            _textSelectOverlordName = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Text_SelectOverlord").GetComponent<TextMeshProUGUI>();
            _textSelectOverlordDescription = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Text_Desc").GetComponent<TextMeshProUGUI>();
            
            _imageSelectOverlordGlow = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Image_Glow").GetComponent<Image>();
            _imageSelectOverlordPortrait = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Image_OverlordPortrait").GetComponent<Image>();            
            _imageCross = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Image_cross").GetComponent<Image>();            
            
            _elementImageDictionary.Clear();
            for (int i=0; i<NumberOfOverlord;++i)
            {
                Image overlordIcon = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Group_DeckIcon/Button_DeckIcon_" + i).GetComponent<Image>();
                Sprite sprite = GameClient.Get<IUIManager>().GetPopup<DeckSelectionPopup>().GetDeckIconSprite
                (
                    _dataManager.CachedHeroesData.Heroes[i].HeroElement
                );
                overlordIcon.sprite = sprite;
                
                _selectOverlordIconList.Add
                (
                    overlordIcon.transform
                );
                
                int index = i;
                Button overlordButton = overlordIcon.GetComponent<Button>();
                overlordButton.onClick.AddListener
                (() =>
                {
                    ChangeOverlordIndex(index);
                    PlayClickSound();
                });

                string elementName = _dataManager.CachedHeroesData.Heroes[i].HeroElement.ToString().ToLower();
                Image elementImage = _selfPage.transform.Find
                (
                    "Tab_SelectOverlord/Panel_Content/Group_Elements/Image_Element_"+elementName
                ).GetComponent<Image>();
                _elementImageDictionary.Add(_dataManager.CachedHeroesData.Heroes[i].HeroElement, elementImage);
            }
        }
        
        public void Update()
        {

        }
        
        public void Dispose()
        {
            _selectOverlordIconList.Clear();   
        }

        #region Button Handlers

        private void ButtonSelectOverlordLeftArrowHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonSelectOverlordLeftArrow.name))
                return;

            PlayClickSound();
            int newIndex = _selectOverlordIndex - 1;
            if (newIndex < 0)
            {
                newIndex = _selectOverlordIconList.Count - 1;
            }
            ChangeOverlordIndex(newIndex);
        }

        private void ButtonSelectOverlordRightArrowHandler()
        {
            if (GameClient.Get<ITutorialManager>().BlockAndReport(_buttonSelectOverlordRightArrow.name))
                return;

            PlayClickSound();
            int newIndex = _selectOverlordIndex + 1;
            if (newIndex >= _selectOverlordIconList.Count)
            {
                newIndex = 0;
            }
            ChangeOverlordIndex(newIndex);
        }
        
        private void ButtonSelectOverlordContinueHandler()
        {
            PlayClickSound();
            _buttonSelectOverlordContinue.interactable = false;
            _myDeckPage.CurrentEditHero = _dataManager.CachedHeroesData.Heroes[_selectOverlordIndex];
            _myDeckPage.AssignCurrentDeck(true);
            ProcessAddDeck();            
        }

        #endregion
        
        private async void ProcessAddDeck()
        {
            bool success = true;
            _myDeckPage.CurrentEditDeck.HeroId = _myDeckPage.CurrentEditHero.HeroId;
            _myDeckPage.CurrentEditDeck.PrimarySkill = _myDeckPage.CurrentEditHero.PrimarySkill;
            _myDeckPage.CurrentEditDeck.SecondarySkill = _myDeckPage.CurrentEditHero.SecondarySkill;

            try
            {
                long newDeckId = await _backendFacade.AddDeck(_backendDataControlMediator.UserDataModel.UserId, _myDeckPage.CurrentEditDeck);
                _myDeckPage.CurrentEditDeck.Id = newDeckId;
                _dataManager.CachedDecksData.Decks.Add(_myDeckPage.CurrentEditDeck);
                _analyticsManager.SetEvent(AnalyticsManager.EventDeckCreated);
                Log.Info(" ====== Add Deck " + newDeckId + " Successfully ==== ");

                if(_tutorialManager.IsTutorial)
                {
                    _dataManager.CachedUserLocalData.TutorialSavedDeck = _myDeckPage.CurrentEditDeck;
                    await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                }
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogExceptionAsWarning(Log, e);

                success = false;

                if (e is Client.RpcClientException || e is TimeoutException)
                {
                    GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true);
                }
                else
                {
                    _myDeckPage.OpenAlertDialog("Not able to Add Deck: \n" + e.Message);
                }
            }
            
            if (success)
            {
                _dataManager.CachedUserLocalData.LastSelectedDeckId = (int)_myDeckPage.CurrentEditDeck.Id;
                await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);                

                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.HordeSaved);

                _myDeckPage.SelectDeckIndex = _myDeckPage.GetDeckList().IndexOf(_myDeckPage.CurrentEditDeck);
                _myDeckPage.AssignCurrentDeck(false, true);
                _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.Tab.SELECT_OVERLORD_SKILL);
            }
            _buttonSelectOverlordContinue.interactable = true;
        }
        
        private void ChangeOverlordIndex(int newIndex)
        {
            _selectOverlordIndex = newIndex;
            UpdateSelectedOverlordDisplay(_selectOverlordIndex);            
        }
        
        private void UpdateSelectedOverlordDisplay(int selectedOverlordIndex)
        {
            Hero hero = _dataManager.CachedHeroesData.Heroes[selectedOverlordIndex];
            _imageSelectOverlordGlow.transform.position = _selectOverlordIconList[selectedOverlordIndex].position;
            _imageSelectOverlordPortrait.sprite = GetOverlordPortraitSprite
            (
                hero.HeroElement
            );
            _textSelectOverlordName.text = hero.FullName;
            _textSelectOverlordDescription.text = hero.ShortDescription;
            
            Enumerators.Faction againstFaction = _myDeckPage.HordeEditTab.FactionAgainstDictionary
            [
                hero.HeroElement
            ];
            _imageCross.transform.position = _elementImageDictionary[againstFaction].transform.position;        
        }
        
        public Sprite GetOverlordPortraitSprite(Enumerators.Faction heroElement)
        {
            string path = "Images/UI/MyDecks/OverlordPortrait";
            switch(heroElement)
            {
                case Enumerators.Faction.AIR:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_air"); 
                case Enumerators.Faction.FIRE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_fire"); 
                case Enumerators.Faction.EARTH:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_earth"); 
                case Enumerators.Faction.TOXIC:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_toxic"); 
                case Enumerators.Faction.WATER:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_water"); 
                case Enumerators.Faction.LIFE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_life"); 
                default:
                    Log.Info($"No Overlord portrait found for faction {heroElement}");
                    return null;
            }        
        }
        
        public void PlayClickSound()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }
    }
}
