using System.Collections.Generic;
using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Editor.Tools;
using Loom.ZombieBattleground.Protobuf;
using UnityEditor;
using UnityEngine;

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

            GUILayout.Label("Match Type: " + matchManager.MatchType);

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition, false, false))
            {
                _scrollPosition = scrollView.scrollPosition;
                EditorGUILayout.BeginVertical();
                {
                    bool isExpanded = true;
                    GameState currentGameState =
                        matchManager.MatchType == Enumerators.MatchType.PVP ?
                        GameStateConstructor.Create().CreateCurrentGameStateFromOnlineGame(false) :
                        GameStateConstructor.Create().CreateCurrentGameStateFromLocalGame(false);
                    GameStateGUI.DrawGameState(currentGameState, userId, "Current Game State", null, ref isExpanded);

                    BattlegroundController battlegroundController = GameClient.Get<IGameplayManager>().GetController<BattlegroundController>();

                    void DrawViewList(IReadOnlyList<IBoardUnitView> views)
                    {
                        if (views.Count == 0)
                        {
                            GUILayout.Label("<i>None</i>",GameStateGUI.Styles.RichLabel);
                            return;
                        }

                        foreach (IBoardUnitView view in views)
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                GUILayout.Label(
                                    view.GetType().Name + ": " + GameStateGUI.FormatCardInstance(view.Model.Card.ToProtobuf()),
                                    GameStateGUI.Styles.RichLabel,
                                    GUILayout.ExpandWidth(false)
                                    );
                                EditorGUILayout.ObjectField(view.Transform.gameObject, typeof(GameObject), true, GUILayout.Width(200));
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }

                    GUILayout.Label("<b>BattlegroundController: All Registered Views</b>", GameStateGUI.Styles.RichLabel);
                    DrawViewList(battlegroundController.BoardUnitViews);

                    GUILayout.Label("<b>BattlegroundController.PlayerHandCards</b>", GameStateGUI.Styles.RichLabel);
                    DrawViewList(battlegroundController.PlayerHandCards);

                    GUILayout.Label("<b>BattlegroundController.OpponentHandCards</b>", GameStateGUI.Styles.RichLabel);
                    DrawViewList(battlegroundController.OpponentHandCards);

                    GUILayout.Label("<b>BattlegroundController.PlayerGraveyardCards</b>", GameStateGUI.Styles.RichLabel);
                    DrawViewList(battlegroundController.PlayerGraveyardCards);

                    GUILayout.Label("<b>BattlegroundController.OpponentGraveyardCards</b>", GameStateGUI.Styles.RichLabel);
                    DrawViewList(battlegroundController.OpponentGraveyardCards);
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
