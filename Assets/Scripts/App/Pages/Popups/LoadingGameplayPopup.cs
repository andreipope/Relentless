// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class LoadingGameplayPopup : IUIPopup
    {
        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private IScenesManager _sceneManager;

        private Image _progressBar;

        public GameObject Self { get; private set; }

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
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/LoadingGameplayPopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _progressBar = Self.transform.Find("ProgresBar/Fill").GetComponent<Image>();

            _progressBar.fillAmount = 0f;
        }

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {
            if (Self == null)

                return;

            _progressBar.fillAmount = Mathf.Max(_progressBar.fillAmount, _sceneManager.SceneLoadingProgress / 100f);

            if (_sceneManager.SceneLoadingProgress >= 100)
            {
                Hide();
            }
        }
    }
}
