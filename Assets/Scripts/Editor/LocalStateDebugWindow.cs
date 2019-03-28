using System;
using System.Collections.Generic;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Editor.Tools;
using Loom.ZombieBattleground.Protobuf;
using UnityEditor;
using UnityEngine;
using Player = Loom.ZombieBattleground.Player;

namespace Editor
{
    public class LocalStateDebugWindow : EditorWindow
    {
        private const double RepaintInterval = 0.2;
        private Vector2 _scrollPosition;
        private double _nextRepaintTime;

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
            if (!EditorApplication.isPlaying || GameClient.Instance == null)
            {
                GUILayout.Label("Only available during gameplay");
                return;
            }

            IMatchManager matchManager = GameClient.Get<IMatchManager>();
            IPvPManager pvpManager = GameClient.Get<IPvPManager>();
            IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
            IAppStateManager appStateManager = GameClient.Get<IAppStateManager>();
            BackendDataControlMediator backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();

            string userId = backendDataControlMediator?.UserDataModel?.UserId;

            if (userId == null)
            {
                GUILayout.Label("User ID not set");
                return;
            }

            if (matchManager.MatchType == Enumerators.MatchType.PVP &&
                (pvpManager.InitialGameState == null ||
                gameplayManager?.CurrentPlayer?.InitialPvPPlayerState == null ||
                gameplayManager.OpponentPlayer?.InitialPvPPlayerState == null))
            {
                GUILayout.Label("PvP match not yet set up");
                return;
            }

            if (appStateManager.AppState != Enumerators.AppState.GAMEPLAY ||
                gameplayManager?.CurrentPlayer == null ||
                gameplayManager.OpponentPlayer == null)
            {
                GUILayout.Label("Match not yet started");
                return;
            }

            BattlegroundController battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();

            GUILayout.Label("Match Type: " + matchManager.MatchType);

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition, false, false))
            {
                _scrollPosition = scrollView.scrollPosition;
                EditorGUILayout.BeginVertical();
                {
                    void DrawViewList<T>(string title, IReadOnlyList<T> items, bool drawType = false)
                    {
                        GUILayout.Label($"<b>{title} ({typeof(T).Name})</b>", GameStateGUI.Styles.RichLabel);
                        if (items.Count == 0)
                        {
                            GUILayout.Label("<i>None</i>",GameStateGUI.Styles.RichLabel);
                            return;
                        }

                        foreach (T item in items)
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                BoardUnitModel model;
                                IBoardUnitView view = item as IBoardUnitView;
                                if (view != null)
                                {
                                    model = view.Model;
                                }
                                else
                                {
                                    model = item as BoardUnitModel;
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

                        //DrawViewList("BoardItemsInUse", player.BoardItemsInUse);
                        //EditorGUILayout.Space();

                        DrawViewList("CardsPreparingToHand", player.CardsPreparingToHand);
                        EditorGUILayout.Space();
                    }

                    bool isExpanded = true;
                    GameState currentGameState =
                        matchManager.MatchType == Enumerators.MatchType.PVP ?
                        GameStateConstructor.Create().CreateCurrentGameStateFromOnlineGame(false) :
                        GameStateConstructor.Create().CreateCurrentGameStateFromLocalGame(false);
                    GameStateGUI.DrawGameState(currentGameState, userId, "Current Game State", null, AfterPlayerDrawnHandlerCallback, ref isExpanded);

                    DrawViewList("BattlegroundController: All Registered Views", battlegroundController.BoardUnitViews, true);
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
