// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoomNetwork.CZB.Gameplay;

namespace LoomNetwork.CZB
{
    public class LoadingGameplayPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private IScenesManager _sceneManager;


        private GameObject _selfPage;

        private Image _progressBar;


        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _sceneManager = GameClient.Get<IScenesManager>();
        }


        public void Dispose()
        {
            
        }

        public void Hide()
        {
            OnHidePopupEvent?.Invoke();

            if (_selfPage == null)
                return;
            
            _selfPage.SetActive (false);
            GameObject.Destroy (_selfPage);
            _selfPage = null;
		}

        public void SetMainPriority()
        {
            
        }

        public void Show()
        {
            _selfPage = MonoBehaviour.Instantiate (_loadObjectsManager.GetObjectByPath<GameObject> ("Prefabs/UI/Popups/LoadingGameplayPopup"));
            _selfPage.transform.SetParent (_uiManager.Canvas3.transform, false);
                
            _progressBar = _selfPage.transform.Find("ProgresBar/Fill").GetComponent<Image>();

            _progressBar.fillAmount = 0f;
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
            if (_selfPage == null)
                return;
            
            _progressBar.fillAmount = (float)_sceneManager.SceneLoadingProgress / 100f;
        }

    }
}