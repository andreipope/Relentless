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
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class MyDecksPage : IUIElement
    {
        private IUIManager _uiManager;
        
        private ILoadObjectsManager _loadObjectsManager;
        
        private GameObject _selfPage;
        
        #region IUIElement
        
        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
        }

        public void Update()
        {
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/MyDecksPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);
            
            _uiManager.DrawPopup<SideMenuPopup>(SideMenuPopup.MENU.MY_DECKS);
            _uiManager.DrawPopup<AreaBarPopup>();
        }
        
        public void Hide()
        {
            Dispose();
        
            if (_selfPage == null)
                return;
        
            _selfPage.SetActive(false);
            Object.Destroy(_selfPage);
            _selfPage = null;
            
            _uiManager.HidePopup<SideMenuPopup>();
            _uiManager.HidePopup<AreaBarPopup>();
        }
        
        public void Dispose()
        {          
        }
        
        #endregion
    }
}
