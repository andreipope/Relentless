using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using UnityEngine;
using UnityEngine.UI;

namespace LoomNetwork.CZB
{
    public class UIManager : IService, IUIManager
    {
        private List<IUIPopup> _uiPopups;

        public List<IUIElement> Pages { get; private set; }

        public IUIElement CurrentPage { get; set; }

        public CanvasScaler CanvasScaler { get; set; }

        public GameObject Canvas { get; set; }

        public GameObject Canvas2 { get; set; }

        public GameObject Canvas3 { get; set; }

        public void Dispose()
        {
            foreach (IUIElement page in Pages)
            {
                page.Dispose();
            }

            foreach (IUIPopup popup in _uiPopups)
            {
                popup.Dispose();
            }
        }

        public void Init()
        {
            Canvas = GameObject.Find("Canvas1");
            Canvas2 = GameObject.Find("Canvas2");
            Canvas3 = GameObject.Find("Canvas3");
            CanvasScaler = Canvas.GetComponent<CanvasScaler>();

            Pages = new List<IUIElement>();
            Pages.Add(new LoadingPage());
            Pages.Add(new MainMenuPage());
            Pages.Add(new HeroSelectionPage());
            Pages.Add(new HordeSelectionPage());
            Pages.Add(new CollectionPage());
            Pages.Add(new DeckEditingPage());
            Pages.Add(new ShopPage());
            Pages.Add(new GameplayPage());
            Pages.Add(new PackOpenerPage());
            Pages.Add(new CreditsPage());

            foreach (IUIElement page in Pages)
            {
                page.Init();
            }

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
            _uiPopups.Add(new TermsPopup());
            _uiPopups.Add(new LoginPopup());
            _uiPopups.Add(new ConnectionPopup());
            _uiPopups.Add(new OverlordAbilitySelectionPopup());
            _uiPopups.Add(new OverlordAbilityTooltipPopup());

            foreach (IUIPopup popup in _uiPopups)
            {
                popup.Init();
            }
        }

        public void Update()
        {
            foreach (IUIElement page in Pages)
            {
                page.Update();
            }

            foreach (IUIPopup popup in _uiPopups)
            {
                popup.Update();
            }
        }

        public void HideAllPages()
        {
            foreach (IUIElement _page in Pages)
            {
                _page.Hide();
            }
        }

        public void SetPage<T>(bool hideAll = false)
            where T : IUIElement
        {
            if (hideAll)
            {
                HideAllPages();
            } else
            {
                if (CurrentPage != null)
                {
                    CurrentPage.Hide();
                }
            }

            foreach (IUIElement _page in Pages)
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

        public void DrawPopup<T>(object message = null, bool setMainPriority = false)
            where T : IUIPopup
        {
            IUIPopup popup = null;
            foreach (IUIPopup _popup in _uiPopups)
            {
                if (_popup is T)
                {
                    popup = _popup;
                    break;
                }
            }

            if (setMainPriority)
            {
                popup.SetMainPriority();
            }

            if (message == null)
            {
                popup.Show();
            } else
            {
                popup.Show(message);
            }
        }

        public void HidePopup<T>()
            where T : IUIPopup
        {
            foreach (IUIPopup _popup in _uiPopups)
            {
                if (_popup is T)
                {
                    _popup.Hide();
                    break;
                }
            }
        }

        public T GetPopup<T>()
            where T : IUIPopup
        {
            IUIPopup popup = null;
            foreach (IUIPopup _popup in _uiPopups)
            {
                if (_popup is T)
                {
                    popup = _popup;
                    break;
                }
            }

            return (T)popup;
        }

        public T GetPage<T>()
            where T : IUIElement
        {
            IUIElement page = null;
            foreach (IUIElement _page in Pages)
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
