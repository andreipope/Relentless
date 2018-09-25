using System;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Gameplay;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Loom.ZombieBattleground
{
    public class YouLosePopup : IUIPopup
    {
        public event Action PopupHiding;

        private ILoadObjectsManager _loadObjectsManager;

        private IUIManager _uiManager;

        private Button _buttonOk;

        private SpriteRenderer _selectHeroSpriteRenderer;

        public GameObject Self { get; private set; }

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
            PopupHiding?.Invoke();
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
            Self = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Popups/YouLosePopup"));
            Self.transform.SetParent(_uiManager.Canvas3.transform, false);

            _selectHeroSpriteRenderer =
                Self.transform.Find("Pivot/YouLosePopup/SelectHero").GetComponent<SpriteRenderer>();

            _buttonOk = Self.transform.Find("Pivot/YouLosePopup/UI/Button_Continue").GetComponent<Button>();
            _buttonOk.onClick.AddListener(OnClickOkButtonEventHandler);

            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.LOST_POPUP, Constants.SfxSoundVolume, false,
                false, true);
            GameClient.Get<ICameraManager>().FadeIn(0.8f, 1);

            int playerDeckId = GameClient.Get<IGameplayManager>().PlayerDeckId;
            int heroId = GameClient.Get<IDataManager>().CachedDecksData.Decks.First(d => d.Id == playerDeckId).HeroId;
            Hero currentPlayerHero = GameClient.Get<IDataManager>().CachedHeroesData.HeroesParsed[heroId];
            string heroName = currentPlayerHero.Element.ToLower();
            _selectHeroSpriteRenderer.sprite =
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/Heroes/hero_" + heroName.ToLower());
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
            GameClient.Get<ISoundManager>()
                .PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);

            GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.HORDE_SELECTION);

            _uiManager.HidePopup<YouLosePopup>();
        }
    }
}
