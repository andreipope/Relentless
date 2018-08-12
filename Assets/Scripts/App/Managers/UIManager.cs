// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class UIManager : IService, IUIManager
    {
        public List<IUIElement> Pages { get { return _uiPages; } }

        private List<IUIElement> _uiPages;
        private List<IUIPopup> _uiPopups;

        public IUIElement CurrentPage { get; set; }

        public CanvasScaler CanvasScaler { get; set; }
		public GameObject Canvas { get; set; }
		public GameObject Canvas2 { get; set; }
		public GameObject Canvas3 { get; set; }

        public void Dispose()
        {
            foreach (var page in _uiPages)
                page.Dispose();

            foreach (var popup in _uiPopups)
                popup.Dispose();
        }

        public void Init()
        {
			Canvas = GameObject.Find("Canvas1");
			Canvas2 = GameObject.Find("Canvas2");
			Canvas3 = GameObject.Find("Canvas3");
            CanvasScaler = Canvas.GetComponent<CanvasScaler>();

            _uiPages = new List<IUIElement>();
			_uiPages.Add(new LoadingPage());
			_uiPages.Add(new MainMenuPage());
			_uiPages.Add(new HeroSelectionPage());
			_uiPages.Add(new HordeSelectionPage());
            _uiPages.Add(new CollectionPage());
            _uiPages.Add(new DeckEditingPage());
            _uiPages.Add(new ShopPage());
            _uiPages.Add(new GameplayPage());
            _uiPages.Add(new PackOpenerPage());
            _uiPages.Add(new CreditsPage());

            foreach (var page in _uiPages)
                page.Init();

            _uiPopups = new List<IUIPopup>();
			_uiPopups.Add(new CardInfoPopup());
            _uiPopups.Add(new DesintigrateCardPopup());
			_uiPopups.Add(new WarningPopup());
			_uiPopups.Add(new QuestionPopup());
            _uiPopups.Add(new TutorialPopup());
            _uiPopups.Add(new PreparingForBattlePopup());
            _uiPopups.Add(new YouLosePopup());
            _uiPopups.Add(new YouWonPopup());
            _uiPopups.Add(new YourTurnPopup());
            _uiPopups.Add(new ConfirmationPopup());
            _uiPopups.Add(new LoadingGameplayPopup());
            _uiPopups.Add(new PlayerOrderPopup());
            _uiPopups.Add(new TermsPopup ());
			_uiPopups.Add(new LoginPopup ());
			_uiPopups.Add(new ConnectionPopup());
            _uiPopups.Add(new OverlordAbilitySelectionPopup());
            _uiPopups.Add(new OverlordAbilityTooltipPopup());

            foreach (var popup in _uiPopups)
                popup.Init();
        }

        public void Update()
        {
            foreach (var page in _uiPages)
                page.Update();

            foreach (var popup in _uiPopups)
                popup.Update();
        }

        public void HideAllPages()
        {
            foreach (var _page in _uiPages)
            {
                _page.Hide();
            }
        }

        public void SetPage<T>(bool hideAll = false) where T : IUIElement
        {
            if (hideAll)
            {
                HideAllPages();
            }
            else
            {
                if (CurrentPage != null)
                    CurrentPage.Hide();
            }

            foreach (var _page in _uiPages)
            {
                if (_page is T)
                {
                    CurrentPage = _page;
                    break;
                }
            }
            CurrentPage.Show();
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SFX_SOUND_VOLUME, false, false, true);
        }

        public void DrawPopup<T>(object message = null, bool setMainPriority = false) where T : IUIPopup
        {
            IUIPopup popup = null;
            foreach (var _popup in _uiPopups)
            {
                if (_popup is T)
                {
                    popup = _popup;
                    break;
                }
            }

            if (setMainPriority)
                popup.SetMainPriority();

            if (message == null)
                popup.Show();
            else
                popup.Show(message);
        }

        public void HidePopup<T>() where T : IUIPopup
        {
            foreach (var _popup in _uiPopups)
            {
                if (_popup is T)
                {
                    _popup.Hide();
                    break;
                }
            }
        }

        public T GetPopup<T>() where T : IUIPopup
        {
            IUIPopup popup = null;
            foreach (var _popup in _uiPopups)
            {
                if (_popup is T)
                {
                    popup = _popup;
                    break;
                }
            }

            return (T)popup;
        }

        public T GetPage<T>() where T : IUIElement
        {
            IUIElement page = null;
            foreach (var _page in _uiPages)
            {
                if (_page is T)
                {
                    page = _page;
                    break;
                }
            }

            return (T)page;
        }
    }
}