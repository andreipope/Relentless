using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
                return _opponentManaBar;
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

        private TextMeshPro _playerDefenseText,
            _opponentDefenseText,
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

        private OverlordModel _playerOverlord,
                     _opponentOverlord;

        private List<GameObject> _cacheBoardUnitViewObjectList;

        private GameObject _cacheBoardUnitViewObjectContainer;

        private const int BoardUnitViewObjectCacheAmount = 5;

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
        
            _cacheBoardUnitViewObjectList = new List<GameObject>();
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

            _playerOverlord = null;
            _opponentOverlord = null;

            UnLoadCachingBoardUnitViewObjects();
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
            
                if(_cacheBoardUnitViewObjectList.Count < BoardUnitViewObjectCacheAmount)
                {
                    GameObject boardUnitViewObject = Object.Instantiate(_loadObjectsManager.GetObjectByPath<GameObject>("Prefabs/Gameplay/BoardCreature"));
                    boardUnitViewObject.transform.SetParent(_cacheBoardUnitViewObjectContainer.transform);
                    _cacheBoardUnitViewObjectList.Add(boardUnitViewObject);
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

            LoadCachingBoardUnitViewObjects();

            StartGame();
            KeepButtonVisibility(false);
        }

        private void LoadCachingBoardUnitViewObjects()
        {
            UnLoadCachingBoardUnitViewObjects();
            _cacheBoardUnitViewObjectContainer = new GameObject("CacheBoardViewContainer");
        }
        
        private void UnLoadCachingBoardUnitViewObjects()
        {
            for(int i = 0; i < _cacheBoardUnitViewObjectList.Count; ++i)
            {
                Object.Destroy(_cacheBoardUnitViewObjectList[i]);
            }
            _cacheBoardUnitViewObjectList.Clear();
            Object.Destroy(_cacheBoardUnitViewObjectContainer);
        }
        
        public GameObject FetchCacheBoardUnitViewObject()
        {
            if (_cacheBoardUnitViewObjectList.Count <= 0)
                return null;

            GameObject boardUnitViewObject = _cacheBoardUnitViewObjectList[0];
            _cacheBoardUnitViewObjectList.Remove(boardUnitViewObject);
            return boardUnitViewObject;
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

            int overlordId = -1;
            int overlordHeroId = -1;

            switch (_matchManager.MatchType)
            {
                case Enumerators.MatchType.LOCAL:
                    if (_gameplayManager.IsTutorial && !_tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.DisabledInitialization)
                    {
                        overlordId = _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.PlayerInfo.OverlordId;
                        overlordHeroId = _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.OpponentInfo.OverlordId;
                    }
                    else
                    {
                        overlordId = _dataManager.CachedDecksData.Decks.First(o => o.Id == CurrentDeckId).OverlordId;

                        List<Data.AIDeck> decks = _dataManager.CachedAiDecksData.Decks.FindAll(x => x.Deck.Cards.Count > 0);

                        Data.AIDeck opponentDeck = _gameplayManager.OpponentIdCheat == -1 ? decks[Random.Range(0, decks.Count)] : decks[_gameplayManager.OpponentIdCheat];


                        overlordHeroId = opponentDeck.Deck.OverlordId;
                        _gameplayManager.OpponentPlayerDeck = opponentDeck.Deck;
                        _gameplayManager.OpponentDeckId = (int)_gameplayManager.OpponentPlayerDeck.Id;

                        _gameplayManager.OpponentIdCheat = -1;

                        if(_gameplayManager.IsTutorial && _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.EnableCustomDeckForOpponent)
                        {
                            _gameplayManager.OpponentPlayerDeck.PrimarySkill =
                                _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.OpponentInfo.PrimarySkill;
                            _gameplayManager.OpponentPlayerDeck.SecondarySkill =
                                _tutorialManager.CurrentTutorial.TutorialContent.ToGameplayContent().SpecificBattlegroundInfo.OpponentInfo.SecondarySkill;

                        }
                    }
                    break;
                case Enumerators.MatchType.PVP:
                    foreach (Protobuf.PlayerState playerState in _pvpManager.InitialGameState.PlayerStates)
                    {
                        if (playerState.Id == _backendDataControlMediator.UserDataModel.UserId)
                        {
                            overlordId = (int)playerState.Deck.OverlordId;
                        }
                        else
                        {
                            overlordHeroId = (int)playerState.Deck.OverlordId;
                            _gameplayManager.OpponentPlayerDeck = playerState.Deck.FromProtobuf();
                            _gameplayManager.OpponentDeckId = -1;
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (overlordId == -1)
                throw new Exception($"{nameof(overlordId)} == -1");

            if (overlordHeroId == -1)
                throw new Exception($"{nameof(overlordHeroId)} == -1");

            _playerOverlord = _dataManager.CachedOverlordData.Overlords[overlordId];
            _opponentOverlord = _dataManager.CachedOverlordData.Overlords[overlordHeroId];

            _playerDeckStatusTexture = GameObject.Find("Player/Deck_Illustration/Deck").GetComponent<SpriteRenderer>();
            _opponentDeckStatusTexture =
                GameObject.Find("Opponent/Deck_Illustration/Deck").GetComponent<SpriteRenderer>();
            _playerGraveyardStatusTexture = GameObject.Find("Player/Graveyard_Illustration/Graveyard")
                .GetComponent<SpriteRenderer>();
            _opponentGraveyardStatusTexture = GameObject.Find("Opponent/Graveyard_Illustration/Graveyard")
                .GetComponent<SpriteRenderer>();

            _playerDefenseText = GameObject.Find("Player/OverlordArea/RegularModel/RegularPosition/Avatar/Deffence/DefenceText").GetComponent<TextMeshPro>();
            _opponentDefenseText = GameObject.Find("Opponent/OverlordArea/RegularModel/RegularPosition/Avatar/Deffence/DefenceText").GetComponent<TextMeshPro>();

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

            if (_playerOverlord != null)
            {
                SetOverlordInfo(_playerOverlord, Constants.Player);
                string playerNameText = _playerOverlord.FullName;
                if (_backendDataControlMediator.LoadUserDataModel())
                {
                    playerNameText = _backendDataControlMediator.UserDataModel.UserId;
                }

                _playerNameText.text = playerNameText;
            }

            if (_opponentOverlord != null)
            {
                SetOverlordInfo(_opponentOverlord, Constants.Opponent);

                _opponentNameText.text = _matchManager.MatchType == Enumerators.MatchType.PVP ?
                                                        _pvpManager.GetOpponentUserId() : _opponentOverlord.FullName;
            }

            _playerManaBar = new PlayerManaBarItem(GameObject.Find("PlayerManaBar"), "GooOverflowPlayer",
                 _playerManaBarsPosition, _playerNameText.text, Constants.Player);
            _opponentManaBar = new PlayerManaBarItem(GameObject.Find("OpponentManaBar"), "GooOverflowOpponent",
                _opponentManaBarsPosition, _opponentNameText.text, Constants.Opponent);

            _isPlayerInited = true;
        }

        public void SetOverlordInfo(OverlordModel overlord, string objectName)
        {
            Texture2D overlordTexture =
                _loadObjectsManager.GetObjectByPath<Texture2D>("Images/Heroes/CZB_2D_Hero_Portrait_" + overlord.Faction + "_EXP");
            Transform overlordObjectTransform = GameObject.Find(objectName + "/OverlordArea/RegularModel/RegularPosition/Avatar/OverlordImage").transform;

            Material overlordAvatarMaterial = new Material(Shader.Find("Sprites/Default"));
            overlordAvatarMaterial.mainTexture = overlordTexture;

            MeshRenderer renderer;
            for (int i = 0; i < overlordObjectTransform.childCount; i++)
            {
                renderer = overlordObjectTransform.GetChild(i).GetComponent<MeshRenderer>();

                if (renderer != null)
                {
                    renderer.material = overlordAvatarMaterial;
                }
            }
        }

        public void SetupSkills(OverlordSkill primary, OverlordSkill secondary, bool isOpponent)
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

        private void SetupSkills(OverlordSkill skillPrim, OverlordSkill skillSecond, GameObject skillPrimary, GameObject skillSecondary)
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

            player.PlayerCardsController.DeckChanged += OnPlayerDeckChangedHandler;
            player.PlayerDefenseChanged += OnPlayerDefenseChanged;
            player.PlayerCurrentGooChanged += OnPlayerCurrentGooChanged;
            player.PlayerGooVialsChanged += OnPlayerGooVialsChanged;
            opponent.PlayerCardsController.DeckChanged += OnOpponentDeckChangedHandler;
            opponent.PlayerDefenseChanged += OnOpponentDefenseChanged;
            opponent.PlayerCurrentGooChanged += OnOpponentCurrentGooChanged;
            opponent.PlayerGooVialsChanged += OnOpponentGooVialsChanged;

            player.TurnStarted += TurnStartedHandler;

            OnPlayerDeckChangedHandler(player.CardsInDeck.Count);
            OnPlayerDefenseChanged(player.Defense);
            OnPlayerGooVialsChanged(player.GooVials, false);
            OnPlayerCurrentGooChanged(player.CurrentGoo);
            OnOpponentDeckChangedHandler(opponent.CardsInDeck.Count);
            OnOpponentDefenseChanged(opponent.Defense);
            OnOpponentGooVialsChanged(opponent.GooVials, false);
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

        private void OnPlayerDefenseChanged(int defense)
        {
            if (!_isPlayerInited)
                return;

            _playerDefenseText.text = defense.ToString();

            if (defense > 9)
            {
                _playerDefenseText.color = Color.white;
            }
            else
            {
                _playerDefenseText.color = Color.red;
            }
        }

        private void OnPlayerCurrentGooChanged(int goo)
        {
            if (!_isPlayerInited)
                return;

            _playerManaBar.SetGoo(goo);
        }

        private void OnPlayerGooVialsChanged(int currentTurnGoo, bool disableAddedBottles)
        {
            if (!_isPlayerInited)
                return;

            _playerManaBar.SetVialGoo(currentTurnGoo, disableAddedBottles);
        }

        private void OnOpponentDefenseChanged(int defense)
        {
            if (!_isPlayerInited)
                return;

            _opponentDefenseText.text = defense.ToString();

            if (defense > 9)
            {
                _opponentDefenseText.color = Color.white;
            }
            else
            {
                _opponentDefenseText.color = Color.red;
            }
        }

        private void OnOpponentCurrentGooChanged(int goo)
        {
            if (!_isPlayerInited)
                return;

            _opponentManaBar.SetGoo(goo);
        }

        private void OnOpponentGooVialsChanged(int currentTurnGoo, bool disableAddedBottles)
        {
            if (!_isPlayerInited)
                return;

            _opponentManaBar.SetVialGoo(currentTurnGoo, disableAddedBottles);
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
            _uiManager.DrawPopup<SettingsWithCreditsPopup>();
            _soundManager.PlaySound(Enumerators.SoundType.CLICK, Constants.SfxSoundVolume, false, false, true);
        }

        #endregion

    }
}
