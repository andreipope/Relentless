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

                GUILayout.Label("<b>Debug Cheats</b>", Styles.RichLabel);
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

                GUILayout.Label("<b>Matchmaking</b>", Styles.RichLabel);
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
                DrawGameState(_initialGameState.Instance, "Initial Game State", ref isExpanded);
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

                if (_currentGameState != null && _currentGameState.HasValue)
                {
                    bool isExpanded = _currentGameState.IsExpanded;
                    DrawGameState(_currentGameState.Instance, "Current Game State", ref isExpanded);
                    _currentGameState.IsExpanded = isExpanded;
                }

                DrawSeparator();

                GUILayout.Label("<b>Action Log</b>", Styles.RichLabel);
                {
                    _playerActionLogView.Draw();
                }
            }
        }

        private void DrawConnectionGui()
        {
            GUILayout.Label("<b>Connection</b>", Styles.RichLabel);
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
            PlayerState currentPlayerState = GetCurrentPlayerState(_currentGameState.Instance);
            PlayerState opponentPlayerState = GetOpponentPlayerState(_currentGameState.Instance);

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
            }
            GUILayout.EndHorizontal();

            // Card Play
            GUILayout.BeginHorizontal();
            {
                IList<CardInstance> cardsInHand = currentPlayerState.CardsInHand;

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
                        await DebugClient.BackendFacade.SendPlayerAction(
                            DebugClient.MatchRequestFactory.CreateAction(
                                DebugClient.PlayerActionFactory.CardPlay(cardsInHand[_gameActionsState.CardPlayCardIndex].InstanceId.FromProtobuf(), 0)
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

                GUILayout.Label("<i>Card Attack: </i>", Styles.RichLabel, GUILayout.ExpandWidth(false));

                DrawMinWidthLabel("Attacker");
                _gameActionsState.CardAttackAttackerIndex =
                    EditorGUILayout.Popup(
                        _gameActionsState.CardAttackAttackerIndex,
                        attackers.Select(SimpleFormatCardInstance).ToArray()
                    );

                DrawMinWidthLabel("Affect Object Type");
                _gameActionsState.CardAttackAffectObjectType =
                    (Enumerators.AffectObjectType) EditorGUILayout.EnumPopup(_gameActionsState.CardAttackAffectObjectType);

                DrawMinWidthLabel("Target");
                _gameActionsState.CardAttackTargetIndex =
                    EditorGUILayout.Popup(
                        _gameActionsState.CardAttackTargetIndex,
                        targets.Select(SimpleFormatCardInstance).ToArray()
                    );

                EditorGUI.BeginDisabledGroup(attackers.Count == 0);
                if (GUILayout.Button("Card Attack"))
                {
                    EnqueueAsyncTask(async () =>
                    {
                        await DebugClient.BackendFacade.SendPlayerAction(
                            DebugClient.MatchRequestFactory.CreateAction(
                                DebugClient.PlayerActionFactory.CardAttack(
                                    new Data.InstanceId(attackers[_gameActionsState.CardAttackAttackerIndex].InstanceId.Id),
                                    _gameActionsState.CardAttackAffectObjectType,
                                    new Data.InstanceId(targets.Count == 0 ? -1 : targets[_gameActionsState.CardAttackTargetIndex].InstanceId.Id)
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

                GUILayout.Label("<i>(Cheat) Card To Destroy</i>", Styles.RichLabel, GUILayout.ExpandWidth(false));
                _gameActionsState.CardToDestroyIndex =
                    EditorGUILayout.Popup(
                        _gameActionsState.CardToDestroyIndex,
                        cardsInPlay.Select(SimpleFormatCardInstance).ToArray()
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
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawGameState(GameState gameState, string stateName, ref bool isExpanded)
        {
            isExpanded = EditorGUILayout.Foldout(isExpanded, stateName);
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
                    $"<b>IId</b>: {cardInstance.InstanceId.Id}, " +
                    $"<b>Name</b>: {cardInstance.Prototype.Name}, " +
                    $"<b>Atk</b>: {cardInstance.Instance.Attack}, " +
                    $"<b>Def</b>: {cardInstance.Instance.Defense}, " +
                    $"<b>Cost</b>: {cardInstance.Instance.GooCost}";
            }

            void DrawPlayer(PlayerState playerState, bool hasCurrentTurn)
            {
                string playerId = playerState.Id;
                if (playerId == DebugClient.UserDataModel?.UserId)
                {
                    playerId = "(Me) " + playerId;
                }

                if (hasCurrentTurn)
                {
                    playerId = "(Current Turn) " + playerId;
                }

                GUILayout.TextField(playerId, GUI.skin.label, GUILayout.MaxWidth(Screen.width / 2 - 50));

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
                        if (playerState.Id == DebugClient.UserDataModel.UserId)
                        {
                            playerState = GetCurrentPlayerState(gameState);
                        }
                        else
                        {
                            playerState = GetOpponentPlayerState(gameState);
                        }
                        DrawPlayer(playerState, i == gameState.CurrentPlayerIndex);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndHorizontal();
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

        private PlayerState GetCurrentPlayerState(GameState gameState)
        {
            PlayerState truePlayerState = gameState.PlayerStates.First(state => state.Id == DebugClient.UserDataModel.UserId);
            if (!DebugClient.UseBackendGameLogic)
            {
                IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
                Player opponentPlayer = gameplayManager.OpponentPlayer;
                if (opponentPlayer == null)
                    return truePlayerState;

                return CreateFakePlayerStateFromPlayer(truePlayerState, opponentPlayer, true);
            }

            return truePlayerState;
        }

        private PlayerState GetOpponentPlayerState(GameState gameState)
        {
            PlayerState truePlayerState = gameState.PlayerStates.First(state => state.Id != DebugClient.UserDataModel.UserId);
            if (!DebugClient.UseBackendGameLogic)
            {
                IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
                Player localPlayer = gameplayManager.CurrentPlayer;
                if (localPlayer == null)
                    return truePlayerState;

                return CreateFakePlayerStateFromPlayer(truePlayerState, localPlayer, false);
            }

            return truePlayerState;
        }

        private static PlayerState CreateFakePlayerStateFromPlayer(PlayerState truePlayerState, Player player, bool useBoardCards)
        {
            PlayerState playerState = new PlayerState
            {
                Id = truePlayerState.Id,
                Defense = player.Defense,
                GooVials = player.GooVials,
                TurnTime = (int) player.TurnTime,
                CardsInPlay =
                {
                    !useBoardCards ? player.CardsOnBoard.Select(card => card.ToProtobuf()).ToArray() : player.BoardCards.Select(card => card.Model.Card.ToProtobuf()).ToArray()
                },
                CardsInDeck =
                {
                    player.CardsInDeck.Select(card => card.ToProtobuf()).ToArray()
                },
                CardsInHand =
                {
                    player.CardsInHand.Select(card => card.ToProtobuf()).ToArray()
                }
            };
            return playerState;
        }

        private static string SimpleFormatCardInstance(CardInstance cardInstance)
        {
            return
                $"IId: {cardInstance.InstanceId.Id}, " +
                $"Name: {cardInstance.Prototype.Name}, " +
                $"Atk: {cardInstance.Instance.Attack}, " +
                $"Def: {cardInstance.Instance.Defense}, " +
                $"Cost: {cardInstance.Instance.GooCost}";
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

        private static class Styles
        {
            public static GUIStyle RichLabel;

            public static GUIStyle BoxLeftAlign;

            public static GUIStyle LabelWithWordWrap;

            static Styles()
            {
                RichLabel = new GUIStyle(EditorStyles.label);
                RichLabel.richText = true;

                BoxLeftAlign = new GUIStyle(GUI.skin.box);
                BoxLeftAlign.alignment = TextAnchor.UpperLeft;

                LabelWithWordWrap = new GUIStyle(GUI.skin.label);
                LabelWithWordWrap.wordWrap = true;
            }
        }

        [Serializable]
        private class GameActionsState
        {
            public int CardPlayCardIndex;

            public int CardAttackAttackerIndex;
            public int CardAttackTargetIndex;
            public Enumerators.AffectObjectType CardAttackAffectObjectType;

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
