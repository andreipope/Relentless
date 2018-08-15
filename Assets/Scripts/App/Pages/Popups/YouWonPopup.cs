// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using LoomNetwork.CZB.Data;
using LoomNetwork.Internal;
using LoomNetwork.CZB.Gameplay;

namespace LoomNetwork.CZB
{
    public class YouWonPopup : IUIPopup
    {
        public GameObject Self
        {
            get { return _selfPage; }
        }

        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;
        private IUIManager _uiManager;
        private GameObject _selfPage,
                            _winTutorialPackObject,
                            _winPackObject;

        private Button _buttonOk;
        private TextMeshProUGUI _message;

        private SpriteRenderer _selectHeroSpriteRenderer;


        //private TextMeshProUGUI _nameHeroText;

        public void Init()
        {
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _uiManager = GameClient.Get<IUIManager>();

            _selfPage = MonoBehaviour.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YouWonPopup"));
            _selfPage.transform.SetParent(_uiManager.Canvas3.transform, false);

            _selectHeroSpriteRenderer = _selfPage.transform.Find("Pivot/YouWonPopup/YouWonPanel/SelectHero").GetComponent<SpriteRenderer>();
            _message = _selfPage.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/Message").GetComponent<TextMeshProUGUI>();
            //_winTutorialPackObject = _selfPage.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/WinPackTutorial").gameObject;
            //_winPackObject = _selfPage.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/WinPack").gameObject;
            //_nameHeroText = _selectHeroImage.transform.Find("Text_NameHero").GetComponent<TextMeshProUGUI>();
            _buttonOk = _selfPage.transform.Find("Pivot/YouWonPopup/YouWonPanel/UI/Button_Continue").GetComponent<Button>();
            _buttonOk.onClick.AddListener(OnClickOkButtonEventHandler);

            _message.text = "Rewards have been disabled for ver " + Constants.CURRENT_VERSION;

            Hide();
        }


        public void Dispose()
        {
        }

        public void Hide()
        {
            OnHidePopupEvent?.Invoke();
            _selfPage.SetActive(false);
			GameClient.Get<ICameraManager>().FadeOut(null, 1);
		}

        public void SetMainPriority()
        {
        }

        public void Show()
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.WON_POPUP, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<ICameraManager>().FadeIn(0.7f, 1);
            _selfPage.SetActive(true);

            int playerDeckId = GameClient.Get<IGameplayManager>().PlayerDeckId;
            int heroId = GameClient.Get<IDataManager>().CachedDecksData.decks.First(d => d.id == playerDeckId).heroId;
            Hero currentPlayerHero = GameClient.Get<IDataManager>().CachedHeroesData.Heroes[heroId];
            string heroName = currentPlayerHero.element.ToString().ToLower();
            _selectHeroSpriteRenderer.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" + heroName.ToLower());
            heroName = Utilites.FirstCharToUpper(heroName);
            //_nameHeroText.text = heroName + " Hero";

            //_winTutorialPackObject.SetActive(GameClient.Get<ITutorialManager>().IsTutorial);
			//_winPackObject.SetActive(!GameClient.Get<ITutorialManager>().IsTutorial);
		}

        public void Show(object data)
        {
            Show();
        }

        public void Update()
        {

        }

        private void OnClickOkButtonEventHandler()
        {
            GameClient.Get<ISoundManager>().PlaySound(Common.Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.DECK_SELECTION);

            _uiManager.HidePopup<YouWonPopup>();
        }

    }
}