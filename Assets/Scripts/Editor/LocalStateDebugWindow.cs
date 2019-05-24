using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Editor.Tools;
using Loom.ZombieBattleground.Protobuf;
using UnityEditor;
using UnityEngine;
using Player = Loom.ZombieBattleground.Player;
using Rect = UnityEngine.Rect;

namespace Loom.ZombieBattleground.Editor
{
    public class LocalStateDebugWindow : EditorWindow
    {
        private const double RepaintInterval = 0.2;
        private Vector2 _scrollPosition;
        private Vector2 _queueLogScrollPosition;
        private double _nextRepaintTime;

        private List<string> _actionQueueStateDumps = new List<string>();

        [NonSerialized]
        private bool _isSubscribedToActionQueueRootStateChanges;

        [MenuItem("Window/ZombieBattleground/Open Local State Debug Window")]
        private static void OpenWindow()
        {
            LocalStateDebugWindow window = GetWindow<LocalStateDebugWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("Local State Debug");
        }

        private void Update()
        {
            double timeSinceStartup = EditorApplication.timeSinceStartup;
            if (timeSinceStartup > _nextRepaintTime)
            {
                _nextRepaintTime = timeSinceStartup + RepaintInterval;
                Repaint();
            }
        }

        private void OnGUI()
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
            if (!EditorApplication.isPlaying || GameClient.Instance == null)
            {
                GUILayout.Label("Only available during gameplay");
                DrawFallbackGui(null);
                return;
            }

            IMatchManager matchManager = GameClient.Get<IMatchManager>();
            IPvPManager pvpManager = GameClient.Get<IPvPManager>();
            IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
            IAppStateManager appStateManager = GameClient.Get<IAppStateManager>();
            BackendDataControlMediator backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            BattlegroundController battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();
            OpponentController opponentController = GameClient.Get<IGameplayManager>().GetController<OpponentController>();
            ActionsQueueController actionsQueueController = GameClient.Get<IGameplayManager>().GetController<ActionsQueueController>();

            string userId = backendDataControlMediator?.UserDataModel?.UserId;

            if (userId == null)
            {
                GUILayout.Label("User ID not set");
                DrawFallbackGui(actionsQueueController);
                return;
            }

            if (matchManager.MatchType == Enumerators.MatchType.PVP &&
                (pvpManager.InitialGameState == null ||
                    gameplayManager?.CurrentPlayer?.InitialPvPPlayerState == null ||
                    gameplayManager.OpponentPlayer?.InitialPvPPlayerState == null))
            {
                GUILayout.Label("PvP match not yet set up");
                DrawFallbackGui(actionsQueueController);
                return;
            }

            if (appStateManager.AppState != Enumerators.AppState.GAMEPLAY ||
                gameplayManager?.CurrentPlayer == null ||
                gameplayManager.OpponentPlayer == null)
            {
                GUILayout.Label("Match not yet started");
                DrawFallbackGui(actionsQueueController);
                return;
            }

            if (!_isSubscribedToActionQueueRootStateChanges)
            {
                actionsQueueController.RootQueue.Changed += RootQueueOnChanged;
                _isSubscribedToActionQueueRootStateChanges = true;
                _actionQueueStateDumps.Clear();
            }

            GUILayout.Label("Match Type: " + matchManager.MatchType);

            void DrawViewList<T>(string title, IReadOnlyList<T> items, bool drawType = false)
            {
                GUILayout.Label($"<b>{title} ({typeof(T).Name})</b>", GameStateGUI.Styles.RichLabel);
                if (items.Count == 0)
                {
                    GUILayout.Label("<i>None</i>", GameStateGUI.Styles.RichLabel);
                    return;
                }

                foreach (T item in items)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        CardModel model;
                        ICardView view = item as ICardView;
                        if (view != null)
                        {
                            model = view.Model;
                        }
                        else
                        {
                            model = item as CardModel;
                        }

                        if (model == null)
                            throw new Exception("model == null");

                        string description = GameStateGUI.FormatCardInstance(model.Card.ToProtobuf());
                        if (drawType)
                        {
                            description = $"<b>Type: </b>{((object) view ?? model).GetType().Name}, {description}";
                        }

                        GUILayout.Label(
                            description,
                            GameStateGUI.Styles.RichLabel,
                            GUILayout.ExpandWidth(false)
                        );

                        if (view != null)
                        {
                            EditorGUILayout.ObjectField(view.GameObject, typeof(GameObject), true, GUILayout.Width(150));
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            void AfterPlayerDrawnHandlerCallback(bool isCurrentPlayer, PlayerState playerState)
            {
                EditorGUILayout.Space();

                Player player = isCurrentPlayer ? gameplayManager.CurrentPlayer : gameplayManager.OpponentPlayer;

                if (matchManager.MatchType == Enumerators.MatchType.PVP && !isCurrentPlayer)
                {
                    DrawViewList("BoardItemsInUse", opponentController.BoardItemsInUse);
                    EditorGUILayout.Space();
                }

                DrawViewList("MulliganCards", player.MulliganCards);
                EditorGUILayout.Space();
            }

            bool isExpanded = true;
            GameState currentGameState =
                matchManager.MatchType == Enumerators.MatchType.PVP ?
                    GameStateConstructor.Create().CreateCurrentGameStateFromOnlineGame(false) :
                    GameStateConstructor.Create().CreateCurrentGameStateFromLocalGame(false);
            GameStateGUI.DrawGameState(currentGameState, userId, "Current Game State", null, AfterPlayerDrawnHandlerCallback, ref isExpanded);

            DrawViewList("BattlegroundController: All Registered Views", battlegroundController.CardViews, true);

            DrawSeparator();

            GUILayout.Label("<b>Current Main Queue State</b>", GameStateGUI.Styles.RichLabel);
            GUILayout.Label(actionsQueueController.RootQueue.ToString());

            DrawSeparator();

            DrawQueueStateLog();
        }

        private void DrawQueueStateLog()
        {
            GUILayout.Label("<b>Main Queue State Log</b>", GameStateGUI.Styles.RichLabel);
            for (int i = 0; i < _actionQueueStateDumps.Count; i++)
            {
                string queueStateDump = _actionQueueStateDumps[i];
                GUILayout.Label("#" + (i + 1));
                GUILayout.Label(queueStateDump);
                GUILayout.Space(20);
            }
        }

        private void DrawFallbackGui(ActionsQueueController actionsQueueController)
        {
            if (actionsQueueController != null && _isSubscribedToActionQueueRootStateChanges)
            {
                actionsQueueController.RootQueue.Changed -= RootQueueOnChanged;
            }

            if (_actionQueueStateDumps.Count > 0)
            {
                DrawQueueStateLog();
            }
        }

        private void RootQueueOnChanged(ActionQueue queue)
        {
            string queueDump = queue.ToString();
            if (_actionQueueStateDumps.Count == 0 || _actionQueueStateDumps.Last() != queueDump)
            {
                _actionQueueStateDumps.Add(queueDump);
            }
        }

        private static void DrawSeparator()
        {
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, 2);
            rect.height = 1;
            EditorGUI.DrawRect(rect, Color.black);
            EditorGUILayout.Space();
        }
    }
}
