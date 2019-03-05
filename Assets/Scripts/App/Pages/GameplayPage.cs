using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hero = Loom.ZombieBattleground.Data.Hero;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class GameplayPage : IUIElement
    {
        public OnBehaviourHandler PlayerPrimarySkillHandler, PlayerSecondarySkillHandler;

        public OnBehaviourHandler OpponentPrimarySkillHandler, OpponentSecondarySkillHandler;

        private const float TubeLoopSoundVolumeKoef = 0.2f;

        private IUIManager _uiManager;

        private ILoadObjectsManager _loadObjectsManager;

        private IDataManager _dataManager;

        private IGameplayManager _gameplayManager;

        private ISoundManager _soundManager;

        private ITutorialManager _tutorialManager;

        private BackendDataControlMediator _backendDataControlMediator;

        private BattlegroundController _battlegroundController;

        private GameObject _selfPage;

        private Button _buttonBack;
        private Button _settingsButton;

        private ButtonShiftingContent _buttonKeep;

        private PlayerManaBarItem _playerManaBar, _opponentManaBar;

        public PlayerManaBarItem PlayerManaBar
        {
            get
            {
                return _playerManaBar;
            }
        }

        public PlayerManaBarItem OpponentManaBar
        {
            get
            {
                return _playerManaBar;
            }
        }

        public GameObject Self
        {
            get
            {
                return _selfPage;
            }
        }

        private Vector3 _playerManaBarsPosition, _opponentManaBarsPosition;

        private List<CardZoneOnBoardStatus> _deckStatus, _graveyardStatus;

        private TextMeshPro _playerHealthText,
            _opponentHealthText,
            _playerCardDeckCountText,
            _opponentCardDeckCountText,
            _playerNameText,
            _opponentNameText;

        private SpriteRenderer _playerDeckStatusTexture,
            _opponentDeckStatusTexture,
            _playerGraveyardStatusTexture,
            _opponentGraveyardStatusTexture;

        private GameObject _zippingVfx;

        private bool _isPlayerInited;

        private PastActionReportPanel _reportGameActionsPanel;

        private GameObject _endTurnButton;

        public int CurrentDeckId { get; set; }

        private IMatchManager _matchManager;

        private IPvPManager _pvpManager;

        private Hero _playerHero,
                     _opponentHero;

        public void Init()
        {
            _uiManager = GameClient.Get<IUIManager>();
            _loadObjectsManager = GameClient.Get<ILoadObjectsManager>();
            _dataManager = GameClient.Get<IDataManager>();
            _gameplayManager = GameClient.Get<IGameplayManager>();
            _soundManager = GameClient.Get<ISoundManager>();
            _tutorialManager = GameClient.Get<ITutorialManager>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _matchManager = GameClient.Get<IMatchManager>();
            _pvpManager = GameClient.Get<IPvPManager>();

            _gameplayManager.GameInitialized += GameInitializedHandler;
            _gameplayManager.GameEnded += GameEndedHandler;

            _matchManager.MatchFinished += MatchFinishedHandler;

            _deckStatus = new List<CardZoneOnBoardStatus>();
            _deckStatus.Add(new CardZoneOnBoardStatus(null, 0));
            _deckStatus.Add(new CardZoneOnBoardStatus(
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_single"), 15));
            _deckStatus.Add(new CardZoneOnBoardStatus(
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_couple"), 40));
            _deckStatus.Add(new CardZoneOnBoardStatus(
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_bunch"), 60));
            _deckStatus.Add(new CardZoneOnBoardStatus(
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/deck_full"), 80));

            _graveyardStatus = new List<CardZoneOnBoardStatus>();
            _graveyardStatus.Add(new CardZoneOnBoardStatus(null, 0));
            _graveyardStatus.Add(new CardZoneOnBoardStatus(
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_single"), 10));
            _graveyardStatus.Add(new CardZoneOnBoardStatus(
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_couple"), 40));
            _graveyardStatus.Add(new CardZoneOnBoardStatus(
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_bunch"), 75));
            _graveyardStatus.Add(new CardZoneOnBoardStatus(
                _loadObjectsManager.GetObjectByPath<Sprite>("Images/BoardCardsStatuses/graveyard_full"), 100));

            _playerManaBarsPosition = new Vector3(-3.55f, 0, -6.07f);
            _opponentManaBarsPosition = new Vector3(9.77f, 0, 4.75f);
        }

        public void Hide()
        {
            _isPlayerInited = false;

            if (_selfPage == null)
                return;

            _selfPage.SetActive(false);
            _reportGameActionsPanel.Dispose();
            _reportGameActionsPanel = null;
            Object.Destroy(_selfPage);
            _selfPage = null;

            _playerHero = null;
            _opponentHero = null;
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            if (_selfPage != null && _selfPage.activeInHierarchy)
            {
                if (_reportGameActionsPanel != null)
                {
                    _reportGameActionsPanel.Update();
                }
            }
        }

        public void Show()
        {
            _selfPage = Object.Instantiate(
                _loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/UI/Pages/GameplayPage"));
            _selfPage.transform.SetParent(_uiManager.Canvas.transform, false);

            _buttonBack = _selfPage.transform.Find("Button_Back").GetComponent<Button>();
            _settingsButton = _selfPage.transform.Find("Button_Settings").GetComponent<Button>();
            _buttonKeep = _selfPage.transform.Find("Button_Keep").GetComponent<ButtonShiftingContent>();

            _buttonBack.onClick.AddListener(BackButtonOnClickHandler);
            _settingsButton.onClick.AddListener(SettingsButtonOnClickHandler);
            _buttonKeep.onClick.AddListener(KeepButtonOnClickHandler);

            _reportGameActionsPanel = new PastActionReportPanel(_selfPage.transform.Find("ActionReportPanel").gameObject);

            if (_zippingVfx == null)
            {
                _zippingVfx = GameObject.Find("Background/Zapping").gameObject;
                _zippingVfx.SetActive(false);
            }

#if !UNITY_ANDROID && !UNITY_IOS
            _settingsButton.gameObject.SetActive(true);
            _buttonBack.gameObject.SetActive(false);
#else
            _buttonBack.gameObject.SetActive(true);
            _settingsButton.gameObject.SetActive(false);
#endif
            if (_gameplayManager.IsTutorial)
            {
                _buttonBack.gameObject.SetActive(false);
                _settingsButton.gameObject.SetActive(false);
            }

            StartGame();
            KeepButtonVisibility(false);
        }

        public void SetEndTurnButtonStatus(bool status)
        {
            _endTurnButton.GetComponent<EndTurnButton>().SetEnabled(status);
        }

        public void StartGame()
        {
            if (_battlegroundController == null)
            {
                _battlegroundController = _gameplayManager.GetController<BattlegroundController>();

                _battlegroundController.PlayerGraveyardUpdated += PlayerGraveyardUpdatedHandler;
                _battlegroundController.OpponentGraveyardUpdated += OpponentGraveyardUpdatedHandler;
            }

            _gameplayManager.PlayerDeckId = CurrentDeckId;

            int heroId = -1;
            int opponentHeroId = -1;

            switch (_matchManager.MatchType)
            {
                case Enumerators.MatchType.LOCAL:
                    if (_gameplayManager.IsTutorial && !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.DisabledInitialization)
                    {
                        heroId = _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.PlayerInfo.OverlordId;
                        opponentHeroId = _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.OpponentInfo.OverlordId;
                    }
                    else
                    {
                        heroId = _dataManager.CachedDecksData.Decks.First(o => o.Id == CurrentDeckId).HeroId;

                        List<Data.AIDeck> decks = _dataManager.CachedAiDecksData.Decks.FindAll(x => x.Deck.Cards.Count > 0);

                        Data.AIDeck opponentDeck = _gameplayManager.OpponentIdCheat == -1 ? decks[Random.Range(0, decks.Count)] : decks[_gameplayManager.OpponentIdCheat];


                        opponentHeroId = opponentDeck.Deck.HeroId;
                        _gameplayManager.OpponentPlayerDeck = opponentDeck.Deck;
                        _gameplayManager.OpponentDeckId = (int)_gameplayManager.OpponentPlayerDeck.Id;

                        _gameplayManager.OpponentIdCheat = -1;

                        if(_gameplayManager.IsTutorial && _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.EnableCustomDeckForOpponent)
                        {
                            _gameplayManager.OpponentPlayerDeck.PrimarySkill =
                                _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.OpponentInfo.PrimaryOverlordAbility;
                            _gameplayManager.OpponentPlayerDeck.SecondarySkill =
                                _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.OpponentInfo.SecondaryOverlordAbility;

                        }
                    }
                    break;
                case Enumerators.MatchType.PVP:
                    foreach (Protobuf.PlayerState playerState in _pvpManager.InitialGameState.PlayerStates)
                    {
                        if (playerState.Id == _backendDataControlMediator.UserDataModel.UserId)
                        {
                            heroId = (int)playerState.Deck.HeroId;
                        }
                        else
                        {
                            opponentHeroId = (int)playerState.Deck.HeroId;
                            _gameplayManager.OpponentPlayerDeck = playerState.Deck.FromProtobuf();
                            _gameplayManager.OpponentDeckId = -1;
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (heroId == -1)
                throw new Exception($"{nameof(heroId)} == -1");

            if (opponentHeroId == -1)
                throw new Exception($"{nameof(opponentHeroId)} == -1");

            _playerHero = _dataManager.CachedHeroesData.Heroes[heroId];
            _opponentHero = _dataManager.CachedHeroesData.Heroes[opponentHeroId];

            _playerDeckStatusTexture = GameObject.Find("Player/Deck_Illustration/Deck").GetComponent<SpriteRenderer>();
            _opponentDeckStatusTexture =
                GameObject.Find("Opponent/Deck_Illustration/Deck").GetComponent<SpriteRenderer>();
            _playerGraveyardStatusTexture = GameObject.Find("Player/Graveyard_Illustration/Graveyard")
                .GetComponent<SpriteRenderer>();
            _opponentGraveyardStatusTexture = GameObject.Find("Opponent/Graveyard_Illustration/Graveyard")
                .GetComponent<SpriteRenderer>();

            _playerHealthText = GameObject.Find("Player/OverlordArea/RegularModel/RegularPosition/Avatar/Deffence/DefenceText").GetComponent<TextMeshPro>();
            _opponentHealthText = GameObject.Find("Opponent/OverlordArea/RegularModel/RegularPosition/Avatar/Deffence/DefenceText").GetComponent<TextMeshPro>();

            // improve find to get it from OBJECTS ON BOARD!!
            _playerNameText = GameObject.Find("Player/NameBoard/NameText").GetComponent<TextMeshPro>();
            _opponentNameText = GameObject.Find("Opponent/NameBoard/NameText").GetComponent<TextMeshPro>();

            _playerCardDeckCountText = GameObject.Find("Player/CardDeckText").GetComponent<TextMeshPro>();
            _opponentCardDeckCountText = GameObject.Find("Opponent/CardDeckText").GetComponent<TextMeshPro>();

            _endTurnButton = GameObject.Find("EndTurnButton/_1_btn_endturn");

            PlayerPrimarySkillHandler =
                GameObject.Find(Constants.Player).transform.Find("Object_SpellPrimary").GetComponent<OnBehaviourHandler>();
            PlayerSecondarySkillHandler =
                GameObject.Find(Constants.Player).transform.Find("Object_SpellSecondary").GetComponent<OnBehaviourHandler>();

            OpponentPrimarySkillHandler =
                GameObject.Find(Constants.Opponent).transform.Find("Object_SpellPrimary").GetComponent<OnBehaviourHandler>();
            OpponentSecondarySkillHandler =
                GameObject.Find(Constants.Opponent).transform.Find("Object_SpellSecondary").GetComponent<OnBehaviourHandler>();

            if (_playerHero != null)
            {
                SetHeroInfo(_playerHero, Constants.Player);
                string playerNameText = _playerHero.FullName;
                if (_backendDataControlMediator.LoadUserDataModel())
                {
                    playerNameText = _backendDataControlMediator.UserDataModel.UserId;
                }

                _playerNameText.text = playerNameText;
            }

            if (_opponentHero != null)
            {
                SetHeroInfo(_opponentHero, Constants.Opponent);

                _opponentNameText.text = _matchManager.MatchType == Enumerators.MatchType.PVP ?
                                                        _pvpManager.GetOpponentUserId() : _opponentHero.FullName;
            }

            _playerManaBar = new PlayerManaBarItem(GameObject.Find("PlayerManaBar"), "GooOverflowPlayer",
                 _playerManaBarsPosition, _playerNameText.text, Constants.Player);
            _opponentManaBar = new PlayerManaBarItem(GameObject.Find("OpponentManaBar"), "GooOverflowOpponent",
                _opponentManaBarsPosition, _opponentNameText.text, Constants.Opponent);

            _isPlayerInited = true;
        }

        public void SetHeroInfo(Hero hero, string objectName)
        {
            Texture2D heroTexture =
                _loadObjectsManager.GetObjectByPath<Texture2D>("Images/Heroes/CZB_2D_Hero_Portrait_" + hero.HeroElement + "_EXP");
            Transform transfHeroObject = GameObject.Find(objectName + "/OverlordArea/RegularModel/RegularPosition/Avatar/OverlordImage").transform;

            Material heroAvatarMaterial = new Material(Shader.Find("Sprites/Default"));
            heroAvatarMaterial.mainTexture = heroTexture;

            MeshRenderer renderer;
            for (int i = 0; i < transfHeroObject.childCount; i++)
            {
                renderer = transfHeroObject.GetChild(i).GetComponent<MeshRenderer>();

                if (renderer != null)
                {
                    renderer.material = heroAvatarMaterial;
                }
            }
        }

        public void SetupSkills(HeroSkill primary, HeroSkill secondary, bool isOpponent)
        {
            if (isOpponent)
            {
                SetupSkills(primary,
                            secondary,
                            OpponentPrimarySkillHandler.gameObject,
                            OpponentSecondarySkillHandler.gameObject);
            }
            else
            {
                SetupSkills(primary,
                            secondary,
                            PlayerPrimarySkillHandler.gameObject,
                            PlayerSecondarySkillHandler.gameObject);
            }
        }

        private void SetupSkills(HeroSkill skillPrim, HeroSkill skillSecond, GameObject skillPrimary, GameObject skillSecondary)
        {
            if (skillPrim != null)
            {
                skillPrimary.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite =
                    _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + skillPrim.IconPath);
            }

            if (skillSecond != null)
            {
                skillSecondary.transform.Find("Icon").GetComponent<SpriteRenderer>().sprite =
                    _loadObjectsManager.GetObjectByPath<Sprite>("Images/OverlordAbilitiesIcons/" + skillSecond.IconPath);
            }
        }


        private void GameEndedHandler(Enumerators.EndGameType endGameType)
        {
            SetEndTurnButtonStatus(true);
        }

        private void MatchFinishedHandler()
        {
            _reportGameActionsPanel?.Clear();
        }

        private int GetPercentFromMaxDeck(int index)
        {
            return 100 * index / (int)Constants.DeckMaxSize;
        }

        private class CardZoneOnBoardStatus
        {
            public readonly int Percent;

            public readonly Sprite StatusSprite;

            public CardZoneOnBoardStatus(Sprite statusSprite, int percent)
            {
                StatusSprite = statusSprite;
                Percent = percent;
            }
        }

        #region event handlers

        private void GameInitializedHandler()
        {
            Player player = _gameplayManager.CurrentPlayer;
            Player opponent = _gameplayManager.OpponentPlayer;

            player.DeckChanged += OnPlayerDeckChangedHandler;
            player.PlayerDefenseChanged += OnPlayerDefenseChanged;
            player.PlayerCurrentGooChanged += OnPlayerCurrentGooChanged;
            player.PlayerGooVialsChanged += OnPlayerGooVialsChanged;
            opponent.DeckChanged += OnOpponentDeckChangedHandler;
            opponent.PlayerDefenseChanged += OnOpponentDefenseChanged;
            opponent.PlayerCurrentGooChanged += OnOpponentCurrentGooChanged;
            opponent.PlayerGooVialsChanged += OnOpponentGooVialsChanged;

            player.TurnStarted += TurnStartedHandler;

            OnPlayerDeckChangedHandler(player.CardsInDeck.Count);
            OnPlayerDefenseChanged(player.Defense);
            OnPlayerGooVialsChanged(player.GooVials);
            OnPlayerCurrentGooChanged(player.CurrentGoo);
            OnOpponentDeckChangedHandler(opponent.CardsInDeck.Count);
            OnOpponentDefenseChanged(opponent.Defense);
            OnOpponentGooVialsChanged(opponent.GooVials);
            OnOpponentCurrentGooChanged(opponent.CurrentGoo);
        }

        private void OnPlayerDeckChangedHandler(int index)
        {
            if (!_isPlayerInited)
                return;

            _playerCardDeckCountText.text = index.ToString();

            if (index == 0)
            {
                _playerDeckStatusTexture.sprite = _deckStatus.Find(x => x.Percent == index).StatusSprite;
            }
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                CardZoneOnBoardStatus nearest =
                    _deckStatus
                        .OrderBy(x => Math.Abs(x.Percent - percent))
                        .First(y => y.Percent > 0);

                _playerDeckStatusTexture.sprite = nearest.StatusSprite;
            }
        }

        private void PlayerGraveyardUpdatedHandler(int index)
        {
            if (!_isPlayerInited)
                return;

            if (index == 0)
            {
                _playerGraveyardStatusTexture.sprite = _graveyardStatus.Find(x => x.Percent == index).StatusSprite;
            }
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                List<CardZoneOnBoardStatus> nearestObjects = _graveyardStatus
                    .OrderBy(x => Math.Abs(x.Percent - percent)).Where(y => y.Percent > 0).ToList();

                CardZoneOnBoardStatus nearest;

                if (nearestObjects[0].Percent > 0)
                {
                    nearest = nearestObjects[0];
                }
                else
                {
                    nearest = nearestObjects[1];
                }

                _playerGraveyardStatusTexture.sprite = nearest.StatusSprite;
            }
        }

        private void OnOpponentDeckChangedHandler(int index)
        {
            if (!_isPlayerInited)
                return;

            _opponentCardDeckCountText.text = index.ToString();

            if (index == 0)
            {
                _opponentDeckStatusTexture.sprite = _deckStatus.Find(x => x.Percent == index).StatusSprite;
            }
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                CardZoneOnBoardStatus nearest = _deckStatus.OrderBy(x => Math.Abs(x.Percent - percent))
                    .Where(y => y.Percent > 0).First();

                _opponentDeckStatusTexture.sprite = nearest.StatusSprite;
            }
        }

        private void OpponentGraveyardUpdatedHandler(int index)
        {
            if (!_isPlayerInited)
                return;

            if (index == 0)
            {
                _opponentGraveyardStatusTexture.sprite = _deckStatus.Find(x => x.Percent == index).StatusSprite;
            }
            else
            {
                int percent = GetPercentFromMaxDeck(index);

                List<CardZoneOnBoardStatus> nearestObjects = _graveyardStatus
                    .OrderBy(x => Math.Abs(x.Percent - percent)).Where(y => y.Percent > 0).ToList();

                CardZoneOnBoardStatus nearest;
                if (nearestObjects[0].Percent > 0)
                {
                    nearest = nearestObjects[0];
                }
                else
                {
                    nearest = nearestObjects[1];
                }

                _opponentGraveyardStatusTexture.sprite = nearest.StatusSprite;
            }
        }

        private void OnPlayerDefenseChanged(int health)
        {
            if (!_isPlayerInited)
                return;

            _playerHealthText.text = health.ToString();

            if (health > 9)
            {
                _playerHealthText.color = Color.white;
            }
            else
            {
                _playerHealthText.color = Color.red;
            }
        }

        private void OnPlayerCurrentGooChanged(int goo)
        {
            if (!_isPlayerInited)
                return;

            _playerManaBar.SetGoo(goo);
        }

        private void OnPlayerGooVialsChanged(int currentTurnGoo)
        {
            if (!_isPlayerInited)
                return;

            _playerManaBar.SetVialGoo(currentTurnGoo);
        }

        private void OnOpponentDefenseChanged(int health)
        {
            if (!_isPlayerInited)
                return;

            _opponentHealthText.text = health.ToString();

            if (health > 9)
            {
                _opponentHealthText.color = Color.white;
            }
            else
            {
                _opponentHealthText.color = Color.red;
            }
        }

        private void OnOpponentCurrentGooChanged(int goo)
        {
            if (!_isPlayerInited)
                return;

            _opponentManaBar.SetGoo(goo);
        }

        private void OnOpponentGooVialsChanged(int currentTurnGoo)
        {
            if (!_isPlayerInited)
                return;

            _opponentManaBar.SetVialGoo(currentTurnGoo);
        }

        private void TurnStartedHandler()
        {
            _zippingVfx.SetActive(_gameplayManager.GetController<PlayerController>().IsActive);

            _soundManager.PlaySound(Enumerators.SoundType.GOO_BOTTLE_FILLING, Constants.SfxSoundVolume);
        }

        #endregion

        #region Buttons Handlers

        public void BackButtonOnClickHandler()
        {
            Action[] actions = new Action[2];
            actions[0] = () =>
            {
                if (_gameplayManager.GetController<CardsController>().CardDistribution)
                {
                    _uiManager.HidePopup<MulliganPopup>();
                }

                GameClient.Get<IAppStateManager>().SetPausingApp(false);
                _uiManager.HidePopup<YourTurnPopup>();

                _gameplayManager.EndGame(Enumerators.EndGameType.CANCEL);
                GameClient.Get<IMatchManager>().FinishMatch(Enumerators.AppState.MAIN_MENU);

                _soundManager.StopPlaying(Enumerators.SoundType.TUTORIAL);
                _soundManager.CrossfaidSound(Enumerators.SoundType.BACKGROUND, null, true);
            };
            actions[1] = () => {
                GameClient.Get<IAppStateManager>().SetPausingApp(false);
            };

            _uiManager.DrawPopup<ConfirmationPopup>(actions);
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
            GameClient.Get<IAppStateManager>().SetPausingApp(true);
        }

        public void KeepButtonOnClickHandler()
        {
            _gameplayManager.GetController<CardsController>().EndCardDistribution();
            KeepButtonVisibility(false);
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        public void KeepButtonVisibility(bool visible)
        {
            _buttonKeep.gameObject.SetActive(visible);
        }

        public void SettingsButtonOnClickHandler()
        {
            _uiManager.DrawPopup<MySettingPopup>();
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        #endregion

    }
}
