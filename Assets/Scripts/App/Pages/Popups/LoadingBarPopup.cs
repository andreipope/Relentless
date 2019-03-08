﻿using System;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class LoadingBarPopup : IUIPopup
    {
        public GameObject Self { get; private set; }
        
        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private TextMeshProUGUI _text;        
        
        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();
        }

        public void Dispose()
        {
        }

        public void Hide()
        {
            if (Self == null)
                return;

            Self.SetActive(false);
            Object.Destroy(Self);
            Self = null;
        }

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            if (Self != null)
                return;

            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/LoadingBarPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _text = Self.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        }

        public void Show(object data)
        {
            Show();

            _text.text = (string) data;
        }

        public void Update()
        {
        }       
    }
}