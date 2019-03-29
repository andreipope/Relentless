using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Test;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor.Tools
{
    public class MultiplayerDebugClientCustomDeckWindow : EditorWindow
    {
        [SerializeField]
        private MultiplayerDebugClientWindow _ownerWindow;

        public MultiplayerDebugClient Client => _ownerWindow.DebugClient;

        private Dictionary<string, string> _cardNameToDescription;
        private bool _visible;
        private Vector2 _customDeckScrollPosition;
        private Vector2 _cardLibraryScrollPosition;

        public void Init(MultiplayerDebugClientWindow ownerWindow)
        {
            _ownerWindow = ownerWindow;
            Preload();
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Custom Deck");
            Preload();
        }

        private bool Preload()
        {
            if (_ownerWindow == null || Client?.CardLibrary == null)
                return false;

            List<Card> cardLibrary = Client.CardLibrary;
            if (Client.DebugCheats.CustomDeck == null)
            {
                Client.DebugCheats.CustomDeck =
                    new Deck(-1, 0, "custom deck", new List<DeckCardData>(), Enumerators.Skill.NONE, Enumerators.Skill.NONE);
            }

            if (_cardNameToDescription == null)
            {
                _cardNameToDescription = new Dictionary<string, string>();
                foreach (Card card in cardLibrary)
                {
                    _cardNameToDescription[card.Name] =
                        $"{card.Name} (set: {card.Faction}, cost: {card.Cost}, atk: {card.Damage}, def: {card.Defense})";
                }
            }

            return true;
        }

        private void OnGUI()
        {
            if (!Preload())
            {
                EditorGUILayout.LabelField("Client not ready");
                return;
            }

            List<Card> cardLibrary = Client.CardLibrary;
            Deck customDeck = Client.DebugCheats.CustomDeck;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Custom Deck", EditorStyles.boldLabel);
                    _customDeckScrollPosition = EditorGUILayout.BeginScrollView(_customDeckScrollPosition);
                    {
                        foreach (DeckCardData deckCard in customDeck.Cards)
                        {
                            DrawCustomDeckCard(customDeck, deckCard);
                        }
                    }
                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Set All Amounts to 0"))
                        {
                            foreach (DeckCardData card in customDeck.Cards)
                            {
                                card.Amount = 0;
                            }
                        }

                        if (GUILayout.Button("Remove All"))
                        {
                            customDeck.Cards.Clear();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField("Card Library", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical();
                    {
                        _cardLibraryScrollPosition = EditorGUILayout.BeginScrollView(_cardLibraryScrollPosition, GUILayout.MaxHeight(300f));
                        {
                            foreach (Card card in cardLibrary.OrderBy(card => card.Faction).ThenBy(card => card.Name))
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    GUILayout.Label(_cardNameToDescription[card.Name]);

                                    GUILayout.FlexibleSpace();
                                    if (GUILayout.Button("Add", GUILayout.Width(70)))
                                    {
                                        if (!customDeck.Cards.Any(deckCard => deckCard.CardName == card.Name))
                                        {
                                            DeckCardData deckCardData = new DeckCardData(card.Name, 0);
                                            customDeck.Cards.Add(deckCardData);
                                        }
                                    }
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        EditorGUILayout.EndScrollView();

                        if (GUILayout.Button("Close"))
                        {
                            Close();
                        }
                    }

                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Deck Settings", EditorStyles.boldLabel);
                    GUILayout.Space(10);

                    GUILayout.Label("Primary Skill");
                    customDeck.PrimarySkill = (Enumerators.Skill) EditorGUILayout.EnumPopup(customDeck.PrimarySkill);

                    GUILayout.Label("Secondary Skill");
                    customDeck.SecondarySkill = (Enumerators.Skill) EditorGUILayout.EnumPopup(customDeck.SecondarySkill);

                    GUILayout.Label("Overlord Id");
                    string overlordIString = GUILayout.TextField(customDeck.OverlordId.ToString());
                    if (int.TryParse(overlordIString, out int newOverlordId))
                    {
                        customDeck.OverlordId = newOverlordId;
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCustomDeckCard(Deck customDeck, DeckCardData deckCard)
        {
            int deckCardIndex = customDeck.Cards.IndexOf(deckCard);

            void MoveCard(int direction)
            {
                int newDeckCardIndex = deckCardIndex + direction;
                DeckCardData otherCard = customDeck.Cards[newDeckCardIndex];
                customDeck.Cards[newDeckCardIndex] = deckCard;
                customDeck.Cards[deckCardIndex] = otherCard;

                GUIUtility.ExitGUI();
            }

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(_cardNameToDescription[deckCard.CardName]);
                GUILayout.FlexibleSpace();
                string amountString = EditorGUILayout.TextField(deckCard.Amount.ToString(), GUILayout.Width(35));
                if (int.TryParse(amountString, out int newAmount))
                {
                    deckCard.Amount = newAmount;
                }
                if (GUILayout.Button("+", GUILayout.Width(30)))
                {
                    deckCard.Amount++;
                }

                if (GUILayout.Button("-", GUILayout.Width(30)))
                {
                    deckCard.Amount = Mathf.Max(deckCard.Amount - 1, 0);
                }

                GUILayout.Space(5f);

                GUI.enabled = deckCardIndex > 0;
                if (GUILayout.Button("↑", GUILayout.Width(30)))
                {
                    MoveCard(-1);
                }

                GUI.enabled = deckCardIndex < customDeck.Cards.Count - 1;
                if (GUILayout.Button("↓", GUILayout.Width(30)))
                {
                    MoveCard(1);
                }

                GUI.enabled = true;

                GUILayout.Space(5f);

                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    customDeck.Cards.Remove(deckCard);
                    GUIUtility.ExitGUI();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
