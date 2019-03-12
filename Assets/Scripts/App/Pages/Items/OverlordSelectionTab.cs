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
                      _imageSelectOverlordPortrait;
                       
        private List<Transform> _selectOverlordIconList;
                       
        private int _selectOverlordIndex;
        
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
            _myDeckPage.EventChangeTab += (HordeSelectionWithNavigationPage.TAB tab) =>
            {
                if (tab != HordeSelectionWithNavigationPage.TAB.SELECT_OVERLORD)
                    return;
                    
                _textSelectOverlordDeckName.text = "NEW DECK";
                ChangeOverlordIndex(0);
            };
        }
        
        public void Show(GameObject selfPage)
        {
            _selfPage = selfPage;
            
            _textSelectOverlordDeckName = _selfPage.transform.Find("Tab_SelectOverlord/Panel_FrameComponents/Upper_Items/Text_DeckName").GetComponent<TextMeshProUGUI>();
           
            _buttonSelectOverlordLeftArrow = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Button_LeftArrow").GetComponent<Button>();
            _buttonSelectOverlordLeftArrow.onClick.AddListener(ButtonSelectOverlordLeftArrowHandler);
            _buttonSelectOverlordLeftArrow.onClick.AddListener(_myDeckPage.PlayClickSound);
            
            _buttonSelectOverlordRightArrow = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Button_RightArrow").GetComponent<Button>();
            _buttonSelectOverlordRightArrow.onClick.AddListener(ButtonSelectOverlordRightArrowHandler);
            _buttonSelectOverlordRightArrow.onClick.AddListener(_myDeckPage.PlayClickSound);
            
            _buttonSelectOverlordContinue = _selfPage.transform.Find("Tab_SelectOverlord/Panel_FrameComponents/Lower_Items/Button_Continue").GetComponent<Button>();
            _buttonSelectOverlordContinue.onClick.AddListener(ButtonSelectOverlordContinueHandler);
            _buttonSelectOverlordContinue.onClick.AddListener(_myDeckPage.PlayClickSound);
            
            _textSelectOverlordName = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Text_SelectOverlord").GetComponent<TextMeshProUGUI>();
            _textSelectOverlordDescription = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Text_Desc").GetComponent<TextMeshProUGUI>();
            
            _imageSelectOverlordGlow = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Image_Glow").GetComponent<Image>();
            _imageSelectOverlordPortrait = _selfPage.transform.Find("Tab_SelectOverlord/Panel_Content/Image_OverlordPortrait").GetComponent<Image>();            
            
            for(int i=0; i<6;++i)
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
                    _myDeckPage.PlayClickSound();
                });
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
            int newIndex = _selectOverlordIndex - 1;
            if (newIndex < 0)
                newIndex = _selectOverlordIconList.Count - 1;
            ChangeOverlordIndex(newIndex);
        }

        private void ButtonSelectOverlordRightArrowHandler()
        {
            int newIndex = _selectOverlordIndex + 1;
            if (newIndex >= _selectOverlordIconList.Count)
                newIndex = 0;
            ChangeOverlordIndex(newIndex);
        }
        
        private void ButtonSelectOverlordContinueHandler()
        {
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
                Debug.Log(" ====== Add Deck " + newDeckId + " Successfully ==== ");

                if(_tutorialManager.IsTutorial)
                {
                    _dataManager.CachedUserLocalData.TutorialSavedDeck = _myDeckPage.CurrentEditDeck;
                    await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                }
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogException(Log, e);

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
                _myDeckPage.AssignCurrentDeck(false);
                _myDeckPage.ChangeTab(HordeSelectionWithNavigationPage.TAB.SELECT_OVERLORD_SKILL);
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
        }
        
        public Sprite GetOverlordPortraitSprite(Enumerators.SetType heroElement)
        {
            string path = "Images/UI/MyDecks/OverlordPortrait";
            switch(heroElement)
            {
                case Enumerators.SetType.AIR:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_air"); 
                case Enumerators.SetType.FIRE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_fire"); 
                case Enumerators.SetType.EARTH:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_earth"); 
                case Enumerators.SetType.TOXIC:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_toxic"); 
                case Enumerators.SetType.WATER:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_water"); 
                case Enumerators.SetType.LIFE:
                    return _loadObjectsManager.GetObjectByPath<Sprite>(path+"/overlord_portrait_life"); 
                default:
                    Debug.Log($"No Overlord portrait found for setType {heroElement}");
                    return null;
            }        
        }
    }
}
