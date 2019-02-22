using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class MyDecksSelectOverlordSkillTab
    {
        private ILoadObjectsManager _loadObjectsManager;
        
        private IDataManager _dataManager;
        
        private ITutorialManager _tutorialManager;
        
        private IAnalyticsManager _analyticsManager;
        
        private BackendFacade _backendFacade;

        private BackendDataControlMediator _backendDataControlMediator;
        
        private MyDecksPage _myDeckPage;
        
        private GameObject _selfPage;
        
        public Image ImageSelectOverlordSkillPortrait;
        
        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            
            _myDeckPage = GameClient.Get<IUIManager>().GetPage<MyDecksPage>();
            _myDeckPage.EventChangeTab += (MyDecksPage.TAB tab) =>
            {
                if (tab != MyDecksPage.TAB.SELECT_OVERLORD_SKILL)
                    return;
            };
        }
        
        public void Show(GameObject selfPage)
        {
            _selfPage = selfPage;
            
            ImageSelectOverlordSkillPortrait = _selfPage.transform.Find("Anchor_BottomRight/Scaler/Tab_SelectOverlordSkill/Panel_Content/Image_OverlordPortrait").GetComponent<Image>();           
           
        }
        
        public void Update()
        {

        }
        
        public void Dispose()
        {

        }        
    }
}