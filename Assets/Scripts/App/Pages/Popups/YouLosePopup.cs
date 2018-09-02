// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System;
using System.Linq;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using LoomNetwork.CZB.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace LoomNetwork.CZB
{
    public class YouLosePopup : IUIPopup
    {
        public static Action OnHidePopupEvent;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private Button _buttonOk;

        private SpriteRenderer _selectHeroSpriteRenderer;

        public GameObject Self { get; private set; }

        // private TextMeshProUGUI _nameHeroText;
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
            OnHidePopupEvent?.Invoke();
            GameClient.Get<ICameraManager>().FadeOut(null, 1);

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
            Self = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YouLosePopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _selectHeroSpriteRenderer = Self.transform.Find("Pivot/YouLosePopup/SelectHero").GetComponent<SpriteRenderer>();

            // _nameHeroText = _selectHeroImage.transform.Find("Text_NameHero").GetComponent<TextMeshProUGUI>();
            _buttonOk = Self.transform.Find("Pivot/YouLosePopup/UI/Button_Continue").GetComponent<Button>();
            _buttonOk.onClick.AddListener(OnClickOkButtonEventHandler);

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.LOST_POPUP, Constants.SFX_SOUND_VOLUME, false, false, true);
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            int playerDeckId = GameClient.Get<IGameplayManager>().PlayerDeckId;
            int heroId = GameClient.Get<IDataManager>().CachedDecksData.decks.First(d => d.id == playerDeckId).heroId;
            Hero currentPlayerHero = GameClient.Get<IDataManager>().CachedHeroesData.Heroes[heroId];
            string heroName = currentPlayerHero.element.ToLower();
            _selectHeroSpriteRenderer.sprite = _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" + heroName.ToLower());

            // heroName = Utilites.FirstCharToUpper(heroName);
            // _nameHeroText.text = heroName + " Hero";
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
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CLICK, Constants.SFX_SOUND_VOLUME, false, false, true);

            GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.DECK_SELECTION);

            _uiManager.HidePopup<YouLosePopup>();
        }
    }
}
