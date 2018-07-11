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
    public class PreparingForBattlePopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private GameObject _selfPage;


        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/PreparingForBattlePopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas3.transform, false);


            Hide();
        }


        public void Dispose()
        {
        }

        public void Hide()
        {
          // GameClient.Get<ICameraManager>().FadeOut(null, 1);
            OnHidePopupEvent?.Invoke();
            _selfPage.SetActive(false);

		}

        public void SetMainPriority()
        {
        }

        public void Show()
        {
          //  GameClient.Get<ICameraManager>().FadeIn(0.7f, 1);
            _selfPage.SetActive(true);
        }

        public void Show(object data)
        {

            Show();
        }

        public void Update()
        {

        }

    }
}