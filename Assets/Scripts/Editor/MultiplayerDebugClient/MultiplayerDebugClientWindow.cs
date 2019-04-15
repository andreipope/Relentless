using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using Loom.ZombieBattleground.Protobuf;
using Loom.ZombieBattleground.Test;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using DebugCheatsConfiguration = Loom.ZombieBattleground.BackendCommunication.DebugCheatsConfiguration;
using Random = UnityEngine.Random;
using Rect = UnityEngine.Rect;

namespace Loom.ZombieBattleground.Editor.Tools
{
    public class MultiplayerDebugClientWindow : EditorWindow
    {
        [SerializeField]
        private PlayerActionLogView _playerActionLogView = new PlayerActionLogView();

        [SerializeField]
        private Vector2 _scrollPosition;

        [SerializeField]
        private bool _isDebugCheatsExpanded;

        private GameStateWrapper _initialGameState;
        private GameStateWrapper _currentGameState;

        private GameActionsState _gameActionsState = new GameActionsState();

        private MultiplayerDebugClientWrapper _debugClientWrapper = new MultiplayerDebugClientWrapper(new MultiplayerDebugClient("Window"));

        private readonly Queue<Func<Task>> _asyncTaskQueue = new Queue<Func<Task>>();
        private readonly SemaphoreSlim _updateSemaphore = new SemaphoreSlim(1);

        public MultiplayerDebugClient DebugClient => _debugClientWrapper?.Instance;

        [MenuItem("Window/ZombieBattleground/Open New Multiplayer Debug Client")]
        private static void OpenWindow()
        {
            MultiplayerDebugClientWindow window = CreateInstance<MultiplayerDebugClientWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("MP Debug Client");
        }

        private void OnEnable()
        {
            EditorApplication.quitting += () =>
            {
                _initialGameState.OnBeforeSerialize();
                _currentGameState.OnBeforeSerialize();
                _debugClientWrapper.OnBeforeSerialize();
            };
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
            await DebugClient.Update();

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
                DrawGui();
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
                    EditorGUI.BeginDisabledGroup(_asyncTaskQueue.Count != 0);
                    DrawMainGui();
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawMainGui()
        {
            DrawConnectionGui();

            if (DebugClient.BackendFacade != null && DebugClient.MatchMakingFlowController != null)
            {
                DrawSeparator();

                GUILayout.Label("<b>Debug Cheats</b>", GameStateGUI.Styles.RichLabel);
                {
                    EditorGUI.BeginDisabledGroup(DebugClient.MatchMakingFlowController.IsMatchmakingInProcess);
                    {
                        bool isExpanded = _isDebugCheatsExpanded;
                        DrawDebugCheatsConfiguration(DebugClient.DebugCheats, ref isExpanded);
                        _isDebugCheatsExpanded = isExpanded;
                    }
                    EditorGUI.EndDisabledGroup();
                }

                DrawSeparator();

                GUILayout.Label("<b>Matchmaking</b>", GameStateGUI.Styles.RichLabel);
                {
                    EditorGUI.BeginDisabledGroup(DebugClient.MatchMakingFlowController.IsMatchmakingInProcess);
                    {
                        EditorGUIUtility.labelWidth = 175;
                        DebugClient.PvPTags =
                            EditorGUILayout.TextField(
                                    "PvP Tags (separate with |)",
                                    String.Join("|", DebugClient.PvPTags)
                                )
                                .Split(new [] {'|'}, StringSplitOptions.RemoveEmptyEntries)
                                .ToList();
                        EditorGUIUtility.labelWidth = 0;
                        DebugClient.UseBackendGameLogic = EditorGUILayout.ToggleLeft("Use Backend Game Logic", DebugClient.UseBackendGameLogic);
                    }
                    EditorGUI.EndDisabledGroup();

                    GUILayout.Label("State: " + DebugClient.MatchMakingFlowController.State);

                    if (!DebugClient.MatchMakingFlowController.IsMatchmakingInProcess)
                    {
                        if (GUILayout.Button("Start matchmaking"))
                        {
                            EnqueueAsyncTask(async () =>
                            {
                                await DebugClient.MatchMakingFlowController.Start(1, null, DebugClient.PvPTags, DebugClient.UseBackendGameLogic, DebugClient.DebugCheats);
                            });
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Stop matchmaking"))
                        {
                            EnqueueAsyncTask(DebugClient.MatchMakingFlowController.Stop);
                        }
                    }

                    if (DebugClient.MatchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed)
                    {
                        GUILayout.Label(
                            "Match Metadata:\n" +
                            $"  Id: {DebugClient.MatchMakingFlowController.MatchMetadata.Id}\n" +
                            $"  Topics: {String.Join("", "", DebugClient.MatchMakingFlowController.MatchMetadata.Topics)}\n" +
                            $"  UseBackendGameLogic: {DebugClient.MatchMakingFlowController.MatchMetadata.UseBackendGameLogic}"
                        );
                    }
                }

                if (DebugClient.MatchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed &&
                    _currentGameState != null)
                {
                    DrawSeparator();

                    GUILayout.Label("<b>Game Actions</b>", GameStateGUI.Styles.RichLabel);
                    {
                        DrawGameActions();
                    }
                }
            }

            if (_initialGameState != null && _initialGameState.HasValue)
            {
                DrawSeparator();
                bool isExpanded = _initialGameState.IsExpanded;
                GameStateGUI.DrawGameState(_initialGameState.Instance, DebugClient.UserDataModel?.UserId, "Initial Game State", null, null, ref isExpanded);
                _initialGameState.IsExpanded = isExpanded;
            }

            if (DebugClient.MatchMakingFlowController != null && DebugClient.MatchMakingFlowController.State == MatchMakingFlowController.MatchMakingState.Confirmed ||
                _playerActionLogView.PlayerActions.Count > 0)
            {
                DrawSeparator();
                if (GUILayout.Button("Update Game State"))
                {
                    EnqueueAsyncTask(UpdateCurrentGameState);
                }

                if (_currentGameState != null && _currentGameState.HasValue && (!DebugClient.UseBackendGameLogic || GameClient.Instance != null))
                {
                    bool isExpanded = _currentGameState.IsExpanded;
                    GameStateGUI.DrawGameState(
                        _currentGameState.Instance,
                        DebugClient.UserDataModel?.UserId,
                        "Current Game State",
                        playerState =>
                            GameStateGUI.GetPlayerState(_currentGameState.Instance,
                                DebugClient.UserDataModel?.UserId,
                                playerState.Id == DebugClient.UserDataModel?.UserId,
                                DebugClient.UseBackendGameLogic),
                        null,
                        ref isExpanded);
                    _currentGameState.IsExpanded = isExpanded;
                }

                DrawSeparator();

                GUILayout.Label("<b>Action Log</b>", GameStateGUI.Styles.RichLabel);
                {
                    _playerActionLogView.Draw();
                }
            }
        }

        private void DrawConnectionGui()
        {
            GUILayout.Label("<b>Connection</b>", GameStateGUI.Styles.RichLabel);
            {
                if (DebugClient.BackendFacade == null)
                {
                    if (GUILayout.Button("Create Client"))
                    {
                        EnqueueAsyncTask(ResetClient);
                        EnqueueAsyncTask(StartClient);
                    }
                }
                else
                {
                    if (DebugClient.UserDataModel != null)
                    {
                        GUILayout.Label("User ID: " + DebugClient.UserDataModel.UserId);
                    }

                    if (DebugClient.BackendFacade.Contract != null)
                    {
                        GUILayout.Label("State: " + DebugClient.BackendFacade.Contract.Client.ReadClient.ConnectionState);
                    }

                    if (GUILayout.Button("Kill client"))
                    {
                        EnqueueAsyncTask(ResetClient);
                    }
                }
            }
        }

        private void DrawGameActions()
        {
            PlayerState currentPlayerState = GameStateGUI.GetPlayerState(_currentGameState.Instance, DebugClient.UserDataModel.UserId, true, DebugClient.UseBackendGameLogic);
            PlayerState opponentPlayerState = GameStateGUI.GetPlayerState(_currentGameState.Instance, DebugClient.UserDataModel.UserId, false, DebugClient.UseBackendGameLogic);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("End Turn"))
                {
                    EnqueueAsyncTask(async () =>
                    {
                        await DebugClient.BackendFacade.SendPlayerAction(
                            DebugClient.MatchRequestFactory.CreateAction(DebugClient.PlayerActionFactory.EndTurn())
                        );

                        await UpdateCurrentGameState();
                    });
                }

                if (GUILayout.Button("Leave Match"))
                {
                    EnqueueAsyncTask(async () =>
                    {
                        await DebugClient.BackendFacade.SendPlayerAction(
                            DebugClient.MatchRequestFactory.CreateAction(DebugClient.PlayerActionFactory.LeaveMatch())
                        );

                        await UpdateCurrentGameState();
                    });
                }

                if (GUILayout.Button("Handshake"))
                {
                    EnqueueAsyncTask(async () =>
                    {
                        List<Data.InstanceId> cardsInHandForMulligan = new List<Data.InstanceId>();
                        foreach (CardInstance card in opponentPlayerState.CardsInHand) 
                        {
                            cardsInHandForMulligan.Add(card.InstanceId.FromProtobuf());
                        }

                        await DebugClient.BackendFacade.SendPlayerAction(
                            DebugClient.MatchRequestFactory.CreateAction(DebugClient.PlayerActionFactory.Mulligan(cardsInHandForMulligan))
                        );

                        await UpdateCurrentGameState();
                    });
                }
            }
            GUILayout.EndHorizontal();

            // Card Play
            GUILayout.BeginHorizontal();
            {
                IList<CardInstance> cardsInHand = currentPlayerState.CardsInHand;

                GUILayout.Label("<i>Card To Play</i>", GameStateGUI.Styles.RichLabel, GUILayout.ExpandWidth(false));
                _gameActionsState.CardPlayCardIndex =
                    EditorGUILayout.Popup(
                        _gameActionsState.CardPlayCardIndex,
                        cardsInHand.Select(card => GameStateGUI.FormatCardInstance(card, false)).ToArray()
                    );

                int[] cardPlayPositions = Enumerable.Range(0, currentPlayerState.CardsInPlay.Count + 1).ToArray();

                EditorGUIUtility.labelWidth = 50;
                if (!cardPlayPositions.Contains(_gameActionsState.CardPlayPosition))
                {
                    _gameActionsState.CardPlayPosition = 0;
                }

                _gameActionsState.CardPlayPosition =
                    EditorGUILayout.IntPopup(
                        "Position",
                        _gameActionsState.CardPlayPosition,
                        cardPlayPositions.Select(i => i.ToString()).ToArray(),
                        cardPlayPositions.ToArray(),
                        GUILayout.Width(130f)
                    );

                EditorGUI.BeginDisabledGroup(cardsInHand.Count == 0);
                if (GUILayout.Button("Card Play"))
                {
                    EnqueueAsyncTask(async () =>
                    {
                        await DebugClient.BackendFacade.SendPlayerAction(
                            DebugClient.MatchRequestFactory.CreateAction(
                                DebugClient.PlayerActionFactory.CardPlay(cardsInHand[_gameActionsState.CardPlayCardIndex].InstanceId.FromProtobuf(), _gameActionsState.CardPlayPosition)
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
                IList<CardInstance> attackers = currentPlayerState.CardsInPlay;
                IList<CardInstance> targets = opponentPlayerState.CardsInPlay;

                GUILayout.Label("<i>Card Attack: </i>", GameStateGUI.Styles.RichLabel, GUILayout.ExpandWidth(false));

                DrawMinWidthLabel("Attacker");
                _gameActionsState.CardAttackAttackerIndex =
                    EditorGUILayout.Popup(
                        _gameActionsState.CardAttackAttackerIndex,
                        attackers.Select(card => GameStateGUI.FormatCardInstance(card, false)).ToArray()
                    );

                DrawMinWidthLabel("Target");
                _gameActionsState.CardAttackTargetIndex =
                    EditorGUILayout.Popup(
                        _gameActionsState.CardAttackTargetIndex,
                        new[] { "Enemy Overlord", "Own Overlord" }.Concat(
                                targets.Select(card => GameStateGUI.FormatCardInstance(card, false)))
                            .ToArray()
                    );

                EditorGUI.BeginDisabledGroup(attackers.Count == 0);
                if (GUILayout.Button("Card Attack"))
                {
                    EnqueueAsyncTask(async () =>
                    {
                        int attackTargetInstanceId;
                        if (_gameActionsState.CardAttackTargetIndex == 0 || _gameActionsState.CardAttackTargetIndex == 1)
                        {
                            // enemy overlord id
                            PlayerState firstPlayerState = _initialGameState.Instance.PlayerStates[_initialGameState.Instance.CurrentPlayerIndex];
                            attackTargetInstanceId =
                                firstPlayerState.Id == _debugClientWrapper.Instance.UserDataModel.UserId ? 1 : 0;
                            if (_gameActionsState.CardAttackTargetIndex == 1)
                            {
                                attackTargetInstanceId = 1 - attackTargetInstanceId;
                            }
                        }
                        else
                        {
                            attackTargetInstanceId = targets.Count == 0 ? -1 : targets[_gameActionsState.CardAttackTargetIndex-2].InstanceId.Id;
                        }

                        await DebugClient.BackendFacade.SendPlayerAction(
                            DebugClient.MatchRequestFactory.CreateAction(
                                DebugClient.PlayerActionFactory.CardAttack(
                                    new Data.InstanceId(attackers[_gameActionsState.CardAttackAttackerIndex].InstanceId.Id),
                                    new Data.InstanceId(attackTargetInstanceId)
                                )
                            )
                        );

                        await UpdateCurrentGameState();
                    });
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();

            // Cheats Card Destroy
            GUILayout.BeginHorizontal();
            {
                IList<CardInstance> cardsInPlay =
                    currentPlayerState.CardsInPlay
                        .Concat(opponentPlayerState.CardsInPlay)
                        .ToList();

                GUILayout.Label("<i>(Cheat) Card To Destroy</i>", GameStateGUI.Styles.RichLabel, GUILayout.ExpandWidth(false));
                _gameActionsState.CardToDestroyIndex =
                    EditorGUILayout.Popup(
                        _gameActionsState.CardToDestroyIndex,
                        cardsInPlay.Select(card => GameStateGUI.FormatCardInstance(card, false)).ToArray()
                    );

                EditorGUI.BeginDisabledGroup(cardsInPlay.Count == 0);
                if (GUILayout.Button("Destroy"))
                {
                    EnqueueAsyncTask(async () =>
                    {
                        await DebugClient.BackendFacade.SendPlayerAction(
                            DebugClient.MatchRequestFactory.CreateAction(
                                DebugClient.PlayerActionFactory.CheatDestroyCardsOnBoard(new []{ new Data.InstanceId(cardsInPlay[_gameActionsState.CardToDestroyIndex].InstanceId.Id) })
                            )
                        );

                        await UpdateCurrentGameState();
                    });
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();
        }

        private async Task UpdateCurrentGameState()
        {
            GetGameStateResponse getGameStateResponse =
                await DebugClient.BackendFacade.GetGameState(DebugClient.MatchMakingFlowController.MatchMetadata.Id);
            bool isExpanded = _currentGameState.IsExpanded;
            _currentGameState = new GameStateWrapper(getGameStateResponse.GameState);
            _currentGameState.IsExpanded = isExpanded;
        }

        private async Task ResetClient()
        {
            _playerActionLogView = new PlayerActionLogView();
            _initialGameState = null;
            _currentGameState = null;

            if (DebugClient.BackendFacade != null)
            {
                DebugClient.BackendFacade.PlayerActionDataReceived -= OnPlayerActionDataReceived;
                await DebugClient.Reset();
            }
        }

        private void DrawDebugCheatsConfiguration(DebugCheatsConfiguration debugCheats, ref bool isExpanded)
        {
            isExpanded = EditorGUILayout.Foldout(isExpanded, "Debug Cheats");
            if (!isExpanded)
                return;

            using (new EditorGUI.IndentLevelScope())
            {
                debugCheats.Enabled = EditorGUILayout.ToggleLeft("Enabled", debugCheats.Enabled);
                EditorGUI.BeginDisabledGroup(!debugCheats.Enabled);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUI.BeginChangeCheck();
                        bool useCustomRandomSeed = EditorGUILayout.ToggleLeft("Use Custom Random Seed", debugCheats.CustomRandomSeed != null, GUILayout.ExpandWidth(false));
                        bool changed = EditorGUI.EndChangeCheck();
                        if (changed)
                        {
                            if (useCustomRandomSeed && debugCheats.CustomRandomSeed == null)
                            {
                                debugCheats.CustomRandomSeed = 0;
                            }
                            else if (!useCustomRandomSeed)
                            {
                                debugCheats.CustomRandomSeed = null;
                            }
                        }

                        if (debugCheats.CustomRandomSeed != null)
                        {
                            debugCheats.CustomRandomSeed = EditorGUILayout.LongField(debugCheats.CustomRandomSeed.Value);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        debugCheats.UseCustomDeck = EditorGUILayout.ToggleLeft("Use Custom Deck", debugCheats.UseCustomDeck, GUILayout.ExpandWidth(false));

                        EditorGUI.BeginDisabledGroup(!debugCheats.UseCustomDeck);
                        {
                            if (GUILayout.Button("Edit", GUILayout.Width(80f)))
                            {
                                MultiplayerDebugClientCustomDeckWindow customDeckWindow = GetWindow<MultiplayerDebugClientCustomDeckWindow>(GetType());
                                customDeckWindow.Init(this);
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndHorizontal();

                    debugCheats.DisableDeckShuffle = EditorGUILayout.ToggleLeft("Disable Deck Shuffling", debugCheats.DisableDeckShuffle);
                    debugCheats.IgnoreGooRequirements = EditorGUILayout.ToggleLeft("Ignore Goo Requirements", debugCheats.IgnoreGooRequirements);
                    debugCheats.SkipMulligan = EditorGUILayout.ToggleLeft("Skip Mulligan", debugCheats.SkipMulligan);
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private async void OnMatchConfirmed(MatchMetadata metadata)
        {
            DebugClient.MatchRequestFactory = new MatchRequestFactory(metadata.Id);
            DebugClient.PlayerActionFactory = new PlayerActionFactory(DebugClient.UserDataModel.UserId);
            GetGameStateResponse getGameStateResponse = await DebugClient.BackendFacade.GetGameState(metadata.Id);

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

        private void OnPlayerActionDataReceived(byte[] data)
        {
            PlayerActionEvent playerActionEvent = PlayerActionEvent.Parser.ParseFrom(data);
            bool? isLocalPlayer =
                playerActionEvent.PlayerAction != null ?
                    playerActionEvent.PlayerAction.PlayerId == DebugClient.UserDataModel.UserId :
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

        private async Task StartClient()
        {
            await DebugClient.Start(
                contract => new DefaultContractCallProxy(contract),
                new DAppChainClientConfiguration(),
                matchMakingFlowController =>
                {
                    matchMakingFlowController.MatchConfirmed += OnMatchConfirmed;
                },
                backendFacade =>
                {
                    backendFacade.PlayerActionDataReceived += OnPlayerActionDataReceived;
                }
            );
        }

        private void EnqueueAsyncTask(Func<Task> task)
        {
            _asyncTaskQueue.Enqueue(task);
            Repaint();
        }

        private static void DrawMinWidthLabel(string text)
        {
            GUIContent guiContent = new GUIContent(text);
            GUILayout.Label(guiContent, GUILayout.Width(GUI.skin.label.CalcSize(guiContent).x));
        }

        private static void DrawSeparator()
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, 2);
            rect.height = 1;
            EditorGUI.DrawRect(rect, Color.black);
            EditorGUILayout.Space();
        }

        [Serializable]
        private class GameActionsState
        {
            public int CardPlayCardIndex;
            public int CardPlayPosition;

            public int CardAttackAttackerIndex;
            public int CardAttackTargetIndex;

            public int CardToDestroyIndex;
        }

        [Serializable]
        private abstract class JsonUnitySerializationWrapper<T> : ISerializationCallbackReceiver
        {
            [SerializeField]
            protected string _json;

            protected T _instance;

            public T Instance => _instance;

            public bool HasValue => !String.IsNullOrEmpty(_json) || _instance != null;

            protected JsonUnitySerializationWrapper(T instance)
            {
                _instance = instance;
            }

            public abstract void OnBeforeSerialize();

            public abstract void OnAfterDeserialize();
        }

        [Serializable]
        private class JsonUnityProtobufSerializationWrapper<T> : JsonUnitySerializationWrapper<T>
        {
            protected JsonUnityProtobufSerializationWrapper(T instance) : base(instance)
            {
            }

            public override void OnBeforeSerialize()
            {
                _json = _instance?.ToString();
            }

            public override void OnAfterDeserialize()
            {
                if (String.IsNullOrEmpty(_json))
                    return;

                _instance = (T) GameState.Descriptor.Parser.ParseJson(_json);
            }
        }

        [Serializable]
        private class JsonUnityNewtonsoftSerializationWrapper<T> : JsonUnitySerializationWrapper<T>
        {
            protected JsonUnityNewtonsoftSerializationWrapper(T instance) : base(instance)
            {
            }

            public override void OnBeforeSerialize()
            {
                _json = JsonConvert.SerializeObject(_instance);
            }

            public override void OnAfterDeserialize()
            {
                if (String.IsNullOrEmpty(_json))
                    return;

                _instance = JsonConvert.DeserializeObject<T>(_json);
            }
        }

        [Serializable]
        private class GameStateWrapper : JsonUnityProtobufSerializationWrapper<GameState>
        {
            public bool IsExpanded;

            public GameStateWrapper(GameState instance) : base(instance)
            {
            }
        }

        [Serializable]
        private class MultiplayerDebugClientWrapper : JsonUnityNewtonsoftSerializationWrapper<MultiplayerDebugClient>
        {
            public MultiplayerDebugClientWrapper(MultiplayerDebugClient instance) : base(instance)
            {
            }
        }

        [Serializable]
        private class DebugCheatsConfigurationWrapper : JsonUnityNewtonsoftSerializationWrapper<DebugCheatsConfiguration>
        {
            public bool IsExpanded;

            public DebugCheatsConfigurationWrapper(DebugCheatsConfiguration instance) : base(instance)
            {
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

                    Rect rowRect = GUILayoutUtility.GetRect(0, 1000000000, 20, 20, GameStateGUI.Styles.BoxLeftAlign);
                    PostprocessCellRect(ref rowRect);
                    GUI.Label(rowRect, "", GameStateGUI.Styles.BoxLeftAlign);

                    Rect idRect = _header.GetCellRect(IdColumnIndex, rowRect);
                    PostprocessCellRect(ref idRect);
                    GUI.Label(idRect, playerAction.Id, GameStateGUI.Styles.BoxLeftAlign);

                    Rect matchRect = _header.GetCellRect(MatchColumnIndex, rowRect);
                    PostprocessCellRect(ref matchRect);
                    if (GUI.Button(matchRect, playerAction.MatchPreview, GameStateGUI.Styles.BoxLeftAlign))
                    {
                        OpenInDataPreviewWindow(playerAction.Match);
                    }

                    Rect actionTypeRect = _header.GetCellRect(ActionTypeColumnIndex, rowRect);
                    PostprocessCellRect(ref actionTypeRect);
                    GUI.Label(actionTypeRect, playerAction.ActionType, GameStateGUI.Styles.BoxLeftAlign);

                    if (playerAction.HasLocalPlayer)
                    {
                        Rect isLocalPlayerRect = _header.GetCellRect(IsLocalPlayerColumnIndex, rowRect);
                        isLocalPlayerRect.xMin += 10;
                        isLocalPlayerRect.yMin += 2;
                        EditorGUI.Toggle(isLocalPlayerRect, playerAction.IsLocalPlayer);
                    }

                    Rect actionRect = _header.GetCellRect(ActionColumnIndex, rowRect);
                    PostprocessCellRect(ref actionRect);
                    if (GUI.Button(actionRect, playerAction.ActionPreview, GameStateGUI.Styles.BoxLeftAlign))
                    {
                        OpenInDataPreviewWindow(playerAction.Action);
                    }
                }

                if (_playerActions.Count > 0 && GUILayout.Button("Clear", GUILayout.Width(100f)))
                {
                    _playerActions.Clear();
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

                MultiplayerDebugClientDataPreviewWindow dataPreviewWindow = GetWindow<MultiplayerDebugClientDataPreviewWindow>(GetType());
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
