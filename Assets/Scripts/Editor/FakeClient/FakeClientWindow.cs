using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client;
using Loom.Google.Protobuf.Collections;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Random = UnityEngine.Random;
using Rect = UnityEngine.Rect;

namespace Loom.ZombieBattleground.Editor.Tools
{
    public class FakeClientWindow : EditorWindow
    {
        [SerializeField]
        private PlayerActionLogView _playerActionLogView = new PlayerActionLogView();

        private readonly Queue<Func<Task>> _asyncTaskQueue = new Queue<Func<Task>>();
        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);

        private BackendFacade _backendFacade;
        private UserDataModel _userDataModel;
        private MatchMakingFlowController _matchMakingFlowController;
        private MatchRequestFactory _matchRequestFactory;
        private PlayerActionFactory _playerActionFactory;
        private Vector2 _scrollPosition;
        private bool _useBackendGameLogic = true;
        private GameStateWrapper _initialGameState;
        private GameStateWrapper _currentGameState;

        private GameActionsState _gameActionsState = new GameActionsState();

        [MenuItem("Window/ZombieBattleground/Open new FakeClient")]
        private static void OpenWindow()
        {
            FakeClientWindow window = CreateInstance<FakeClientWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Fake Client");
        }

        private async void Update()
        {
            await _updateSemaphore.WaitAsync();
            try
            {
                Repaint();
                await Task.Delay((int) (Time.deltaTime * 1000));
                await AsyncUpdate();
            }
            finally
            {
                _updateSemaphore.Release();
            }
        }

        private async Task AsyncUpdate()
        {
            if (_matchMakingFlowController != null)
            {
                await _matchMakingFlowController.Update();
            }

            while (_asyncTaskQueue.Count > 0)
            {
                try
                {
                    Func<Task> taskFunc = _asyncTaskQueue.Peek();
                    await taskFunc();
                }
                finally
                {
                    _asyncTaskQueue.Dequeue();
                }
            }
        }

        private void OnGUI()
        {
            try
            {
                EditorGUI.BeginDisabledGroup(_asyncTaskQueue.Count != 0);
                DrawGui();
                EditorGUI.EndDisabledGroup();
            }
            catch (ArgumentException e) when (e.Message.Contains("Getting control"))
            {
                // Ignore. Related to the async hacks, it's fine for this debug UI
            }
        }

        private void DrawGui()
        {
            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition, false, false))
            {
                _scrollPosition = scrollView.scrollPosition;
                EditorGUILayout.BeginVertical();
                {
                    DrawMainGui();
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawMainGui()
        {
            GUILayout.Label("<b>Connection</b>", Styles.RichLabel);
            {
                if (_backendFacade == null)
                {
                    if (GUILayout.Button("Create client"))
                    {
                        EnqueueAsyncTask(async () =>
                        {
                            await ResetClient();

                            _userDataModel = new UserDataModel(
                                "TestFakeUser_" + Random.Range(int.MinValue, int.MaxValue).ToString().Replace("-", "0") + Time.frameCount,
                                CryptoUtils.GeneratePrivateKey()
                            );

                            BackendFacade backendFacade = new BackendFacade(GameClient.GetDefaultBackendEndpoint())
                            {
                                Logger = Debug.unityLogger
                            };
                            backendFacade.Init();
                            backendFacade.PlayerActionDataReceived += OnPlayerActionDataReceived;
                            await backendFacade.CreateContract(_userDataModel.PrivateKey);
                            try
                            {
                                await backendFacade.SignUp(_userDataModel.UserId);
                            }
                            catch (TxCommitException e) when (e.Message.Contains("user already exists"))
                            {
                                // Ignore
                            }

                            MatchMakingFlowController matchMakingFlowController = new MatchMakingFlowController(backendFacade, _userDataModel);
                            matchMakingFlowController.ActionWaitingTime = 0.3f;
                            matchMakingFlowController.MatchConfirmed += OnMatchConfirmed;

                            _backendFacade = backendFacade;
                            _matchMakingFlowController = matchMakingFlowController;
                        });
                    }
                }
                else
                {
                    if (_userDataModel != null)
                    {
                        GUILayout.Label("User ID: " + _userDataModel.UserId);
                    }

                    if (_backendFacade.Contract != null)
                    {
                        GUILayout.Label("State: " + _backendFacade.Contract.Client.ReadClient.ConnectionState);
                    }

                    if (GUILayout.Button("Kill client"))
                    {
                        EnqueueAsyncTask(async () =>
                        {
                            await ResetClient();
                        });
                    }
                }
            }

            if (_backendFacade != null && _matchMakingFlowController != null)
            {
                DrawSeparator();

                GUILayout.Label("<b>Matchmaking</b>", Styles.RichLabel);
                {
                    EditorGUI.BeginDisabledGroup(_matchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed);
                    {
                        _useBackendGameLogic = EditorGUILayout.ToggleLeft("Use Backend Game Logic", _useBackendGameLogic);
                    }
                    EditorGUI.EndDisabledGroup();

                    GUILayout.Label("State: " + _matchMakingFlowController.State);

                    if (!_matchMakingFlowController.IsMatchmakingInProcess)
                    {
                        if (GUILayout.Button("Start matchmaking"))
                        {
                            EnqueueAsyncTask(async () =>
                            {
                                await _matchMakingFlowController.Start(1, null, null, _useBackendGameLogic);
                            });
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Stop matchmaking"))
                        {
                            EnqueueAsyncTask(async () =>
                            {
                                await _matchMakingFlowController.Stop();
                            });
                        }
                    }

                    if (_matchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed)
                    {
                        GUILayout.Label(
                            "Match Metadata:\n" +
                            $"  Id: {_matchMakingFlowController.MatchMetadata.Id}\n" +
                            $"  Topics: {String.Join("", "", _matchMakingFlowController.MatchMetadata.Topics)}\n" +
                            $"  UseBackendGameLogic: {_matchMakingFlowController.MatchMetadata.UseBackendGameLogic}"
                        );
                    }
                }

                if (_matchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed &&
                    _currentGameState != null)
                {
                    DrawSeparator();

                    GUILayout.Label("<b>Game Actions</b>", Styles.RichLabel);
                    {
                        DrawGameActions();
                    }
                }
            }


            if (_initialGameState != null && _initialGameState.HasValue)
            {
                DrawSeparator();
                bool isExpanded = _initialGameState.IsExpanded;
                DrawGameState(_initialGameState.GameState, "Initial Game State", ref isExpanded);
                _initialGameState.IsExpanded = isExpanded;
            }

            if (_matchMakingFlowController != null && _matchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed ||
                _playerActionLogView.PlayerActions.Count > 0)
            {
                DrawSeparator();
                if (GUILayout.Button("Get Game State"))
                {
                    EnqueueAsyncTask(async () =>
                    {
                        await UpdateCurrentGameState();
                    });
                }

                if (_currentGameState != null && _currentGameState.HasValue)
                {
                    bool isExpanded = _currentGameState.IsExpanded;
                    DrawGameState(_currentGameState.GameState, "Current Game State", ref isExpanded);
                    _currentGameState.IsExpanded = isExpanded;
                }

                DrawSeparator();

                GUILayout.Label("<b>Action Log</b>", Styles.RichLabel);
                {
                    _playerActionLogView.Draw();
                }
            }
        }

        private async Task UpdateCurrentGameState()
        {
            GetGameStateResponse getGameStateResponse =
                await _backendFacade.GetGameState(_matchMakingFlowController.MatchMetadata.Id);
            bool isExpanded = _currentGameState.IsExpanded;
            _currentGameState = new GameStateWrapper(getGameStateResponse.GameState);
            _currentGameState.IsExpanded = isExpanded;
        }

        private void DrawGameState(GameState gameState, string name, ref bool isExpanded)
        {
            isExpanded = EditorGUILayout.Foldout(isExpanded, name);
            if (!isExpanded)
                return;

            string FormatCardInstances(IList<CardInstance> cardInstances)
            {
                if (cardInstances.Count == 0)
                    return "<i>None</i>";

                return String.Join("\n", cardInstances.Select(FormatCardInstance));
            }

            string FormatCardInstance(CardInstance cardInstance)
            {
                return
                    $"<b>IId</b>: {cardInstance.InstanceId.InstanceId_}, " +
                    $"<b>Name</b>: {cardInstance.Prototype.Name}, " +
                    $"<b>Atk</b>: {cardInstance.Instance.Attack}, " +
                    $"<b>Def</b>: {cardInstance.Instance.Defense}, " +
                    $"<b>Cost</b>: {cardInstance.Instance.GooCost}";
            }

            void DrawPlayer(PlayerState playerState, bool hasCurrentTurn)
            {
                string playerId = playerState.Id;
                if (playerId == _userDataModel?.UserId)
                {
                    playerId = "(Me) " + playerId;
                }

                if (hasCurrentTurn)
                {
                    playerId = playerId + " (Current Turn)";
                }

                GUILayout.Label(playerId);

                EditorGUILayout.Space();
                GUILayout.Label("<b>Stats</b>", Styles.RichLabel);

                GUILayout.Label("Defense: " + playerState.Defense);
                GUILayout.Label("GooVials: " + playerState.CurrentGoo);
                GUILayout.Label("CurrentGoo: " + playerState.GooVials);

                EditorGUILayout.Space();
                GUILayout.Label("<b>Status</b>", Styles.RichLabel);

                GUILayout.Label("CurrentAction: " + playerState.CurrentAction);
                GUILayout.Label("HasDrawnCard: " + playerState.HasDrawnCard);

                EditorGUILayout.Space();
                GUILayout.Label("<b>Meta Info</b>", Styles.RichLabel);

                GUILayout.Label("MaxGooVials: " + playerState.MaxGooVials);
                GUILayout.Label("MaxCardsInHand: " + playerState.MaxCardsInHand);
                GUILayout.Label("MaxCardsInPlay: " + playerState.MaxCardsInPlay);
                GUILayout.Label("InitialCardsInHandCount: " + playerState.InitialCardsInHandCount);

                EditorGUILayout.Space();
                GUILayout.Label("<b>Cards In Play</b>", Styles.RichLabel);

                GUILayout.Label(FormatCardInstances(playerState.CardsInPlay), Styles.RichLabel);

                EditorGUILayout.Space();
                GUILayout.Label("<b>Cards In Hand</b>", Styles.RichLabel);

                GUILayout.Label(FormatCardInstances(playerState.CardsInHand), Styles.RichLabel);

                EditorGUILayout.Space();
                GUILayout.Label("<b>Cards In Deck</b>", Styles.RichLabel);

                GUILayout.Label(FormatCardInstances(playerState.CardsInDeck), Styles.RichLabel);

                EditorGUILayout.Space();
                GUILayout.Label("<b>Cards In Graveyard</b>", Styles.RichLabel);

                GUILayout.Label(FormatCardInstances(playerState.CardsInGraveyard), Styles.RichLabel);
            }

            GUILayout.Label("RandomSeed: " + gameState.RandomSeed);
            GUILayout.Label("CurrentPlayerIndex: " + gameState.CurrentPlayerIndex);
            GUILayout.Label("Winner: " + gameState.Winner);
            GUILayout.Label("IsEnded: " + gameState.IsEnded);

            EditorGUILayout.BeginHorizontal();
            {
                for (int i = 0; i < gameState.PlayerStates.Count; i++)
                {
                    PlayerState playerState = gameState.PlayerStates[i];
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        DrawPlayer(playerState, i == gameState.CurrentPlayerIndex);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGameActions()
        {
            PlayerState currentPlayerState = GetCurrentPlayerState(_currentGameState.GameState);
            PlayerState opponentPlayerState = GetOpponentPlayerState(_currentGameState.GameState);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("End Turn"))
                {
                    EnqueueAsyncTask(async () =>
                    {
                        await _backendFacade.SendPlayerAction(
                            _matchRequestFactory.CreateAction(_playerActionFactory.EndTurn())
                        );

                        await UpdateCurrentGameState();
                    });
                }

                if (GUILayout.Button("Leave Match"))
                {
                    EnqueueAsyncTask(async () =>
                    {
                        await _backendFacade.SendPlayerAction(
                            _matchRequestFactory.CreateAction(_playerActionFactory.LeaveMatch())
                        );

                        await UpdateCurrentGameState();
                    });
                }
            }
            GUILayout.EndHorizontal();

            /*
            // Draw Card
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Draw Card"))
            {
                EnqueueAsyncTask(async () =>
                {
                    await _backendFacade.SendPlayerAction(
                        _matchRequestFactory.CreateAction(_playerActionFactory.())
                    );

                    await UpdateCurrentGameState();
                });
            }
            GUILayout.EndHorizontal();*/

            // Card Play
            GUILayout.BeginHorizontal();
            {
                IList<CardInstance> cardsInHand = GetCurrentPlayerState(_currentGameState.GameState).CardsInHand;

                GUILayout.Label("<i>Card To Play</i>", Styles.RichLabel, GUILayout.ExpandWidth(false));
                _gameActionsState.CardPlayCardIndex =
                    EditorGUILayout.Popup(
                        _gameActionsState.CardPlayCardIndex,
                        cardsInHand.Select(SimpleFormatCardInstance).ToArray()
                    );

                EditorGUI.BeginDisabledGroup(cardsInHand.Count == 0);
                if (GUILayout.Button("Card Play"))
                {
                    EnqueueAsyncTask(async () =>
                    {
                        await _backendFacade.SendPlayerAction(
                            _matchRequestFactory.CreateAction(
                                _playerActionFactory.CardPlay(cardsInHand[_gameActionsState.CardPlayCardIndex], 0)
                            )
                        );

                        await UpdateCurrentGameState();
                    });
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();

            // Card Attack
            GUILayout.BeginHorizontal();
            {
                IList<CardInstance> ownCards =
                    currentPlayerState.CardsInHand
                        .Concat(currentPlayerState.CardsInPlay)
                        .Concat(currentPlayerState.CardsInDeck)
                        .ToList();
                IList<CardInstance> opponentCards =
                    opponentPlayerState.CardsInHand
                        .Concat(opponentPlayerState.CardsInPlay)
                        .Concat(opponentPlayerState.CardsInDeck)
                        .ToList();

                GUILayout.Label("<i>Card Attack: </i>", Styles.RichLabel, GUILayout.ExpandWidth(false));

                DrawMinWidthLabel("Attacker");
                _gameActionsState.CardAttackAttackerIndex =
                    EditorGUILayout.Popup(
                        _gameActionsState.CardAttackAttackerIndex,
                        ownCards.Select(SimpleFormatCardInstance).ToArray()
                    );

                DrawMinWidthLabel("Affect Object Type");
                _gameActionsState.CardAttackAffectObjectType =
                    (Enumerators.AffectObjectType) EditorGUILayout.EnumPopup(
                        _gameActionsState.CardAttackAffectObjectType
                    );

                DrawMinWidthLabel("Target");
                _gameActionsState.CardAttackTargetIndex =
                    EditorGUILayout.Popup(
                        _gameActionsState.CardAttackTargetIndex,
                        opponentCards.Select(SimpleFormatCardInstance).ToArray()
                    );

                EditorGUI.BeginDisabledGroup(ownCards.Count == 0);
                if (GUILayout.Button("Card Attack"))
                {
                    EnqueueAsyncTask(async () =>
                    {
                        await _backendFacade.SendPlayerAction(
                            _matchRequestFactory.CreateAction(
                                _playerActionFactory.CardAttack(
                                    new Data.InstanceId(ownCards[_gameActionsState.CardAttackAttackerIndex].InstanceId.InstanceId_),
                                    _gameActionsState.CardAttackAffectObjectType,
                                    new Data.InstanceId(opponentCards[_gameActionsState.CardAttackTargetIndex].InstanceId.InstanceId_)
                                )
                            )
                        );

                        await UpdateCurrentGameState();
                    });
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();
        }

        private static void DrawMinWidthLabel(string text)
        {
            GUIContent guiContent = new GUIContent(text);
            GUILayout.Label(guiContent, GUILayout.Width(GUI.skin.label.CalcSize(guiContent).x));
        }

        private async Task ResetClient()
        {
            _playerActionLogView = new PlayerActionLogView();
            _initialGameState = null;
            _currentGameState = null;

            if (_backendFacade != null)
            {
                _backendFacade.PlayerActionDataReceived -= OnPlayerActionDataReceived;
                _backendFacade.Contract?.Client.Dispose();

                _backendFacade = null;

                await _matchMakingFlowController.Stop();
                _matchMakingFlowController = null;
            }
        }

        private async void OnMatchConfirmed(MatchMetadata metadata)
        {
            _matchRequestFactory = new MatchRequestFactory(metadata.Id);
            _playerActionFactory = new PlayerActionFactory(_userDataModel.UserId);
            GetGameStateResponse getGameStateResponse = await _backendFacade.GetGameState(metadata.Id);

            _playerActionLogView.Add(
                new PlayerActionLogView.PlayerActionEventViewModel(
                    _playerActionLogView.PlayerActions.Count,
                    "",
                    "Initial State",
                    getGameStateResponse.GameState.ToString(),
                    null
                )
            );
            _initialGameState = new GameStateWrapper(getGameStateResponse.GameState);
            _currentGameState = new GameStateWrapper(getGameStateResponse.GameState);
        }

        private static void DrawSeparator()
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, 2);
            rect.height = 1;
            EditorGUI.DrawRect(rect, Color.black);
            EditorGUILayout.Space();
        }

        private void OnPlayerActionDataReceived(byte[] data)
        {
            PlayerActionEvent playerActionEvent = PlayerActionEvent.Parser.ParseFrom(data);
            bool? isLocalPlayer =
                playerActionEvent.PlayerAction != null ?
                    playerActionEvent.PlayerAction.PlayerId == _userDataModel.UserId :
                    (bool?) null;

            _playerActionLogView.Add(playerActionEvent, isLocalPlayer);

            if (isLocalPlayer != null && !isLocalPlayer.Value)
            {
                EnqueueAsyncTask(async () =>
                {
                    await UpdateCurrentGameState();
                });
            }
        }

        private void EnqueueAsyncTask(Func<Task> task)
        {
            _asyncTaskQueue.Enqueue(task);
            Repaint();
        }

        private PlayerState GetCurrentPlayerState(GameState gameState)
        {
            return gameState.PlayerStates.First(state => state.Id == _userDataModel.UserId);
        }

        private PlayerState GetOpponentPlayerState(GameState gameState)
        {
            return gameState.PlayerStates.First(state => state.Id != _userDataModel.UserId);
        }

        private static string SimpleFormatCardInstance(CardInstance cardInstance)
        {
            return
                $"IId: {cardInstance.InstanceId.InstanceId_}, " +
                $"Name: {cardInstance.Prototype.Name}, " +
                $"Atk: {cardInstance.Instance.Attack}, " +
                $"Def: {cardInstance.Instance.Defense}, " +
                $"Cost: {cardInstance.Instance.GooCost}";
        }

        private static class Styles
        {
            public static GUIStyle RichLabel;

            public static GUIStyle BoxLeftAlign;

            static Styles()
            {
                RichLabel = new GUIStyle(EditorStyles.label);
                RichLabel.richText = true;

                BoxLeftAlign = new GUIStyle(GUI.skin.box);
                BoxLeftAlign.alignment = TextAnchor.UpperLeft;
            }
        }

        [Serializable]
        private class GameActionsState
        {
            public int CardPlayCardIndex;

            public int CardAttackAttackerIndex;
            public int CardAttackTargetIndex;
            public Enumerators.AffectObjectType CardAttackAffectObjectType;
        }

        [Serializable]
        private class GameStateWrapper : ISerializationCallbackReceiver
        {
            [SerializeField]
            private string _gameStateJson;

            private GameState _gameState;

            public bool IsExpanded;

            public GameState GameState => _gameState;

            public bool HasValue => !String.IsNullOrEmpty(_gameStateJson) || _gameState != null;

            public GameStateWrapper(GameState gameState)
            {
                _gameState = gameState;
            }

            public void OnBeforeSerialize()
            {
                _gameStateJson = GameState?.ToString();
            }

            public void OnAfterDeserialize()
            {
                if (String.IsNullOrEmpty(_gameStateJson))
                    return;

                _gameState = (GameState) GameState.Descriptor.Parser.ParseJson(_gameStateJson);
            }
        }

        [Serializable]
        private class PlayerActionLogView
        {
            private const int IdColumnIndex = 0;
            private const int MatchColumnIndex = 1;
            private const int ActionTypeColumnIndex = 2;
            private const int IsLocalPlayerColumnIndex = 3;
            private const int ActionColumnIndex = 4;

            [NonSerialized]
            private bool _initialized;

            [SerializeField]
            private List<PlayerActionEventViewModel> _playerActions = new List<PlayerActionEventViewModel>();

            [SerializeField]
            private MultiColumnHeaderState _multiColumnHeaderState;

            [SerializeField]
            private MultiColumnHeader _header;

            public List<PlayerActionEventViewModel> PlayerActions => _playerActions;

            public void Draw()
            {
                InitIfNeeded();
                Rect headerRect = EditorGUILayout.GetControlRect(false, 20);
                _header.OnGUI(headerRect, 0);

                for (int i = 0; i < _playerActions.Count; i++)
                {
                    PlayerActionEventViewModel playerAction = _playerActions[i];

                    Rect rowRect = GUILayoutUtility.GetRect(0, 1000000000, 20, 20, Styles.BoxLeftAlign);
                    PostprocessCellRect(ref rowRect);
                    GUI.Label(rowRect, "", Styles.BoxLeftAlign);

                    Rect idRect = _header.GetCellRect(IdColumnIndex, rowRect);
                    PostprocessCellRect(ref idRect);
                    GUI.Label(idRect, playerAction.Id, Styles.BoxLeftAlign);

                    Rect matchRect = _header.GetCellRect(MatchColumnIndex, rowRect);
                    PostprocessCellRect(ref matchRect);
                    if (GUI.Button(matchRect, playerAction.MatchPreview, Styles.BoxLeftAlign))
                    {
                        OpenInDataPreviewWindow(playerAction.Match);
                    }

                    Rect actionTypeRect = _header.GetCellRect(ActionTypeColumnIndex, rowRect);
                    PostprocessCellRect(ref actionTypeRect);
                    GUI.Label(actionTypeRect, playerAction.ActionType, Styles.BoxLeftAlign);

                    if (playerAction.HasLocalPlayer)
                    {
                        Rect isLocalPlayerRect = _header.GetCellRect(IsLocalPlayerColumnIndex, rowRect);
                        isLocalPlayerRect.xMin += 10;
                        isLocalPlayerRect.yMin += 2;
                        EditorGUI.Toggle(isLocalPlayerRect, playerAction.IsLocalPlayer);
                    }

                    Rect actionRect = _header.GetCellRect(ActionColumnIndex, rowRect);
                    PostprocessCellRect(ref actionRect);
                    if (GUI.Button(actionRect, playerAction.ActionPreview, Styles.BoxLeftAlign))
                    {
                        OpenInDataPreviewWindow(playerAction.Action);
                    }
                }
            }

            private void OpenInDataPreviewWindow(string json)
            {
                try
                {
                    json = JToken.Parse(json).ToString(Formatting.Indented);
                }
                catch
                {
                    // Ignore
                }

                FakeClientDataPreviewWindow dataPreviewWindow = GetWindow<FakeClientDataPreviewWindow>(GetType());
                dataPreviewWindow.SetText(json);
            }

            public void Add(PlayerActionEvent actionEvent, bool? isLocalPlayer)
            {
                Add(new PlayerActionEventViewModel(actionEvent, _playerActions.Count, isLocalPlayer));
            }

            public void Add(PlayerActionEventViewModel playerActionEventViewModel)
            {
                _playerActions.Add(playerActionEventViewModel);
            }

            private void InitIfNeeded()
            {
                if (_initialized)
                    return;

                MultiColumnHeaderState headerState =
                    new MultiColumnHeaderState(
                        new[]
                        {
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("#"),
                                headerTextAlignment = TextAlignment.Center,
                                width = 32,
                                minWidth = 32,
                                maxWidth = 32,
                                autoResize = false,
                                allowToggleVisibility = false,
                                canSort = false
                            },
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("Match"),
                                headerTextAlignment = TextAlignment.Left,
                                width = 200,
                                minWidth = 200,
                                autoResize = false,
                                allowToggleVisibility = false,
                                canSort = false
                            },
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("Action Type"),
                                headerTextAlignment = TextAlignment.Left,
                                width = 100,
                                minWidth = 100,
                                maxWidth = 100,
                                autoResize = true,
                                allowToggleVisibility = false,
                                canSort = false
                            },
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("Me"),
                                headerTextAlignment = TextAlignment.Left,
                                width = 30,
                                minWidth = 30,
                                maxWidth = 30,
                                autoResize = false,
                                allowToggleVisibility = false,
                                canSort = false
                            },
                            new MultiColumnHeaderState.Column
                            {
                                headerContent = new GUIContent("Value"),
                                headerTextAlignment = TextAlignment.Left,
                                width = 300,
                                minWidth = 300,
                                autoResize = false,
                                allowToggleVisibility = false,
                                canSort = false
                            }
                        });
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(_multiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(_multiColumnHeaderState, headerState);
                _multiColumnHeaderState = headerState;
                _header = new MultiColumnHeader(_multiColumnHeaderState);

                _initialized = true;
            }

            private static void PostprocessCellRect(ref Rect rect)
            {
                rect.xMin += 2;
                rect.xMax += 3;
            }

            [Serializable]
            internal class PlayerActionEventViewModel
            {
                private const int MaxPreviewTextLength = 256;

                public GUIContent Id;
                public GUIContent ActionType;
                public string Match;
                public string Action;
                public GUIContent MatchPreview;
                public GUIContent ActionPreview;
                public bool HasLocalPlayer;
                public bool IsLocalPlayer;

                public PlayerActionEventViewModel(PlayerActionEvent playerAction, long id, bool? isLocalPlayer)
                    : this(
                        id,
                        playerAction.Match.ToString(),
                        playerAction.PlayerAction != null ? playerAction.PlayerAction.ActionType.ToString() : "Matchmaking",
                        playerAction.PlayerAction != null ? playerAction.PlayerAction.ToString() : "Match Status: " + playerAction.Match.Status,
                        isLocalPlayer
                    )
                {
                }

                public PlayerActionEventViewModel(long id, string match, string actionType, string action, bool? isLocalPlayer)
                {
                    Action = action;
                    Match = match;
                    Id = new GUIContent(id.ToString());

                    string matchPreview = Utilites.LimitStringLength(match, MaxPreviewTextLength);
                    string actionPreview = Utilites.LimitStringLength(action, MaxPreviewTextLength);
                    MatchPreview = new GUIContent(matchPreview, matchPreview);
                    ActionType = new GUIContent(actionType);
                    ActionPreview = new GUIContent(actionPreview, actionPreview);
                    HasLocalPlayer = isLocalPlayer.HasValue;
                    IsLocalPlayer = isLocalPlayer ?? false;
                }
            }
        }
    }
}
