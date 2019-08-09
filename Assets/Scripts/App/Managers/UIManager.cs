using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Loom.ZombieBattleground
{
    public class UIManager : IService, IUIManager
    {
        public List<IUIPopup> UiPopups { get; set; }

        public List<IUIElement> Pages { get; private set; }

        public void Dispose()
        {
            foreach (IUIElement page in Pages)
            {
                page.Dispose();
            }

            foreach (IUIPopup popup in UiPopups)
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
            Pages.Add(new GameplayPage());
            Pages.Add(new PvPSelectionPage());
            Pages.Add(new CustomGameModeListPage());
            Pages.Add(new CustomGameModeCustomUiPage());            
            Pages.Add(new MainMenuWithNavigationPage());
            Pages.Add(new PackOpenerPageWithNavigationBar());
            Pages.Add(new HordeSelectionWithNavigationPage());
            Pages.Add(new ArmyWithNavigationPage());
            Pages.Add(new ShopWithNavigationPage());
            Pages.Add(new LoadingWithAnimationPage());

            foreach (IUIElement page in Pages)
            {
                page.Init();
            }

            UiPopups = new List<IUIPopup>();
            UiPopups.Add(new CardInfoPopup());
            UiPopups.Add(new DesintigrateCardPopup());
            UiPopups.Add(new WarningPopup());
            UiPopups.Add(new QuestionPopup());
            UiPopups.Add(new TutorialAvatarPopup());
            UiPopups.Add(new PreparingForBattlePopup());
            UiPopups.Add(new LevelUpPopup());
            UiPopups.Add(new YourTurnPopup());
            UiPopups.Add(new ConfirmationPopup());
            UiPopups.Add(new LoadingGameplayPopup());
            UiPopups.Add(new PlayerOrderPopup());
            //Hide for current Beta release
            //UiPopups.Add(new TermsPopup());
            UiPopups.Add(new LoginPopup());
            UiPopups.Add(new MatchMakingPopup());
            UiPopups.Add(new ConnectionPopup());         
            UiPopups.Add(new PastActionsPopup());
            UiPopups.Add(new UpdatePopup());
            UiPopups.Add(new MulliganPopup());
            UiPopups.Add(new LoadDataMessagePopup());
            UiPopups.Add(new LoadingOverlayPopup());
            UiPopups.Add(new TutorialProgressInfoPopup());
            UiPopups.Add(new RewardPopup());
            UiPopups.Add(new WaitingForPlayerPopup());
            UiPopups.Add(new TutorialSkipPopup());
            UiPopups.Add(new SideMenuPopup());
            UiPopups.Add(new AreaBarPopup());
            UiPopups.Add(new DeckSelectionPopup());
            UiPopups.Add(new GameModePopup());
            UiPopups.Add(new YouWonYouLostPopup());
            UiPopups.Add(new ElementFilterPopup());
            UiPopups.Add(new CardInfoWithSearchPopup());
            UiPopups.Add(new MySettingPopup());
            UiPopups.Add(new LoadingBarPopup());
            UiPopups.Add(new CreditPopup());
            UiPopups.Add(new SettingsWithCreditsPopup());
            UiPopups.Add(new YouWonYouLostWithRewardPopup());
            UiPopups.Add(new InternetConnectionPopup());
            UiPopups.Add(new SelectOverlordAbilitiesPopup());
            UiPopups.Add(new SelectOverlordPopup());
            UiPopups.Add(new RenamePopup());
            UiPopups.Add(new SelectSkinPopup());

            foreach (IUIPopup popup in UiPopups)
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

            foreach (IUIPopup popup in UiPopups)
            {
                popup.Update();
            }
        }

        public IUIElement CurrentPage { get; set; }

        public CanvasScaler CanvasScaler { get; set; }

        public GameObject Canvas { get; set; }

        public GameObject Canvas2 { get; set; }

        public GameObject Canvas3 { get; set; }

        public void HideAllPages()
        {
            foreach (IUIElement page in Pages)
            {
                page.Hide();
            }
        }

        public void HideAllPopups()
        {
            foreach (IUIPopup popup in UiPopups)
            {
                popup.Hide();
            }
        }

        public void SetPage<T>(bool hideAll = false)
            where T : IUIElement
        {
            if (hideAll)
            {
                HideAllPages();
            }
            else
            {
                CurrentPage?.Hide();
            }

            foreach (IUIElement page in Pages)
            {
                if (page is T)
                {
                    CurrentPage = page;
                    break;
                }
            }

            CurrentPage.Show();
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);

            GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.ScreenChanged);
        }

        public void DrawPopup<T>(object message = null, bool setMainPriority = false)
            where T : IUIPopup
        {
            IUIPopup popup = GetPopup<T>();

            if (setMainPriority)
            {
                popup.SetMainPriority();
            }

            if (message == null)
            {
                popup.Show();
            }
            else
            {
                popup.Show(message);
            }

            if (GameClient.Get<ITutorialManager>().IsTutorial)
            {
                if (popup is WarningPopup || popup is ConnectionPopup || popup is QuestionPopup || popup is LoadingOverlayPopup)
                    return;

                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.ScreenChanged);
            }
        }

        public void HidePopup<T>()
            where T : IUIPopup
        {
            IUIPopup popup = GetPopup<T>();
            popup.Hide();

            GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.PopupClosed);
        }

        public T GetPopup<T>()
            where T : IUIPopup
        {
            for (int i = 0; i < UiPopups.Count; i++)
            {
                if (UiPopups[i] is T popup)
                    return popup;
            }

            return default(T);
        }

        public T GetPage<T>()
            where T : IUIElement
        {
            for (int i = 0; i < Pages.Count; i++)
            {
                if (Pages[i] is T page)
                    return page;
            }

            return default(T);
        }

        public void DrawPopupByName(string name, object data = null)
        {
            foreach (IUIPopup popup in UiPopups)
            {
                if (popup.GetType().Name == name)
                {
                    if (popup.Self != null)
                        break;

                    popup.SetMainPriority();

                    if (data == null)
                    {
                        popup.Show();
                    }
                    else
                    {
                        popup.Show(data);
                    }
                    break;
                }
            }

        }

        public void SetPageByName(string name, bool hideAll = false)
        {
            foreach (IUIElement page in Pages)
            {
                if (page.GetType().Name == name)
                {
                    if (CurrentPage == page)
                        break;

                    if (hideAll)
                    {
                        HideAllPages();
                    }
                    else
                    {
                        CurrentPage?.Hide();
                    }

                    CurrentPage = page;
                    CurrentPage.Show();

                    GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                        false, false, true);

                    GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.ScreenChanged);

                    break;
                }
            }
        }
    }
}
