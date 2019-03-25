using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Protobuf;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor.Tools
{
    public class GameStateGUI
    {
        public delegate void AfterPlayerDrawnHandler(bool isCurrentPlayer, PlayerState playerState);

        public static void DrawGameState(
            GameState gameState,
            string currentPlayerUserId,
            string stateName,
            Func<PlayerState, PlayerState> modifyPlayerStateFunc,
            AfterPlayerDrawnHandler afterPlayerDrawnHandlerCallback,
            ref bool isExpanded)
        {
            isExpanded = EditorGUILayout.Foldout(isExpanded, stateName);
            if (!isExpanded)
                return;

            string FormatCardInstances(IList<CardInstance> cardInstances)
            {
                if (cardInstances.Count == 0)
                    return "<i>None</i>";

                return String.Join("\n", cardInstances.Select(card => FormatCardInstance(card)));
            }

            void DrawPlayer(PlayerState playerState, bool hasCurrentTurn)
            {
                string playerId = playerState.Id;
                if (playerId == currentPlayerUserId)
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

                GUILayout.Label("InstanceId: " + playerState.InstanceId.Id);
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

                afterPlayerDrawnHandlerCallback?.Invoke(playerState.Id == currentPlayerUserId, playerState);
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
                        if (modifyPlayerStateFunc != null)
                        {
                            playerState = modifyPlayerStateFunc(playerState);
                        }

                        DrawPlayer(playerState, i == gameState.CurrentPlayerIndex);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
                
        public static PlayerState GetPlayerState(GameState gameState, string currentPlayerUserId, bool isCurrentPlayer, bool useBackendGameLogic)
        {
            PlayerState truePlayerState = gameState.PlayerStates.First(state => isCurrentPlayer ? state.Id == currentPlayerUserId : state.Id != currentPlayerUserId);
            if (!useBackendGameLogic)
            {
                IGameplayManager gameplayManager = GameClient.Get<IGameplayManager>();
                Player opponentPlayer = isCurrentPlayer ? gameplayManager.OpponentPlayer : gameplayManager.CurrentPlayer;
                if (opponentPlayer == null)
                    return truePlayerState;

                return GameStateConstructor.CreateFakePlayerStateFromPlayer(truePlayerState.Id, opponentPlayer, false);
            }

            return truePlayerState;
        }

        public static string FormatCardInstance(CardInstance cardInstance, bool richText = true)
        {
            string AddItem(string name, object value)
            {
                return richText ? $"<b>{name}:</b> {value}" : $"{name}: {value}";
            }

            return
                AddItem("IId", cardInstance.InstanceId.Id) + ", " +
                AddItem("Name", cardInstance.Prototype.Name) + ", " +
                AddItem("Dmg", cardInstance.Instance.Damage) + ", " +
                AddItem("Def", cardInstance.Instance.Defense) + ", " +
                AddItem("Cost", cardInstance.Instance.Cost);
        }

        public static class Styles
        {
            public static readonly GUIStyle RichLabel;
            public static readonly GUIStyle BoxLeftAlign;
            public static readonly GUIStyle LabelWithWordWrap;

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
    }
}
