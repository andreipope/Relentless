// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LoomNetwork.CZB.Gameplay;
using Random = UnityEngine.Random;

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
        private TextMeshProUGUI _flavorText;


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
            GameClient.Get<ICameraManager>().FadeOut(null, 1, true);

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
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/PreparingForBattlePopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas3.transform, false);
            _flavorText = _selfPage.transform.Find("Image_Machine/Flavor_Text").GetComponent<TextMeshProUGUI>();
            
            SetRandomFlavorText();
        }

        private void SetRandomFlavorText()
        {
            IContentManager contentManger = GameClient.Get<IContentManager>();
            int randomVal = Random.Range(0, contentManger.FlavorTextInfo.Count+1);
            _flavorText.text = contentManger.FlavorTextInfo[randomVal].Description;
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