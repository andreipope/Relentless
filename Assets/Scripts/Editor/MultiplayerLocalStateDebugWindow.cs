using Loom.ZombieBattleground;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Editor.Tools;
using Loom.ZombieBattleground.Protobuf;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class MultiplayerLocalStateDebugWindow : EditorWindow
    {
        private const double RepaintInterval = 0.2;
        private Vector2 _scrollPosition;
        private double _nextRepaintTime;

        [MenuItem("Window/ZombieBattleground/Open Multiplayer Local State Debug Window")]
        private static void OpenWindow()
        {
            MultiplayerLocalStateDebugWindow window = GetWindow<MultiplayerLocalStateDebugWindow>();
            window.Show();
        }

        private void Awake()
        {
            titleContent = new GUIContent("MP State Debug");
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
            if (GameClient.Instance == null)
            {
                GUILayout.Label("Only available during a PvP match");
                return;
            }

            IMatchManager matchManager = GameClient.Get<IMatchManager>();
            IPvPManager pvpManager = GameClient.Get<IPvPManager>();
            IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
            if (matchManager.MatchType != Enumerators.MatchType.PVP ||
                pvpManager.InitialGameState == null ||
                gameplayManager?.CurrentPlayer?.InitialPvPPlayerState == null ||
                gameplayManager.OpponentPlayer?.InitialPvPPlayerState == null)
            {
                GUILayout.Label("Only available during a PvP match");
                return;
            }

            BackendDataControlMediator backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            string userId = backendDataControlMediator?.UserDataModel?.UserId;
            if (userId == null)
            {
                GUILayout.Label("User ID not set");
                return;
            }

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition, false, false))
            {
                _scrollPosition = scrollView.scrollPosition;
                EditorGUILayout.BeginVertical();
                {
                    bool isExpanded = true;
                    GameState currentGameState = GameStateConstructor.Create().CreateCurrentGameState(false);
                    GameStateGUI.DrawGameState(currentGameState, userId, "Current Game State", null, ref isExpanded);
                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}
