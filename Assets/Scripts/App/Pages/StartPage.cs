// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



﻿using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Gameplay;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class StartPage : IUIElement
    {
        private GameObject _selfPage;

        private IUIManager _uiManager;
        private ILoadObjectsManager _loadObjectsManager;
        private INotificationManager _notificationsManager;
        private ILocalizationManager _localizationManager;
        private IPlayerManager _playerManager;
        private IDataManager _dataManager;
        private ITimerManager _timerManager;

        private Button _startGame;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _notificationsManager = GameClient.Get<INotificationManager>();
            _localizationManager = GameClient.Get<ILocalizationManager>();
            _playerManager = GameClient.Get<IPlayerManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _timerManager = GameClient.Get<ITimerManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/StartPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);



            _localizationManager.LanguageWasChangedEvent += LanguageWasChangedEventHandler;
            UpdateLocalization();


            _startGame = _selfPage.transform.Find("Button_Start").GetComponent<Button>();
            _startGame.onClick.AddListener(() => 
            {
              // todo smth
            });

            Hide();
        }

        public void Hide()
        {
            _selfPage.SetActive(false);
        }

        public void Show()
        {
            _selfPage.SetActive(true);
        }

        public void Update()
        {

        }

        public void Dispose()
        {

        }


        private void LanguageWasChangedEventHandler(Enumerators.Language obj)
        {
            UpdateLocalization();
        }

        private void UpdateLocalization()
        {
          //  _loginText.text = _localizationManager.GetUITranslation("KEY_START_SCREEN_LOGIN");
        }
    }
}