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
                    new Deck(-1, 0, "custom deck", new List<DeckCardData>(), Enumerators.OverlordSkill.NONE, Enumerators.OverlordSkill.NONE);
            }

            if (_cardNameToDescription == null)
            {
                _cardNameToDescription = new Dictionary<string, string>();
                foreach (Card card in cardLibrary)
                {
                    _cardNameToDescription[card.Name] =
                        $"{card.Name} (set: {card.CardSetType}, cost: {card.Cost}, atk: {card.Damage}, def: {card.Health})";
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
                            DrawCustomDeckCard(deckCard, out bool isRemoved);
                            if (isRemoved)
                            {
                                customDeck.Cards.Remove(deckCard);
                                GUIUtility.ExitGUI();
                            }
                        }
                    }
                    EditorGUILayout.EndScrollView();

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Reset"))
                        {
                            foreach (DeckCardData card in customDeck.Cards)
                            {
                                card.Amount = 0;
                            }
                        }

                        if (GUILayout.Button("Close"))
                        {
                            Close();
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField("Card Library", EditorStyles.boldLabel);
                    _cardLibraryScrollPosition = EditorGUILayout.BeginScrollView(_cardLibraryScrollPosition, GUILayout.MaxHeight(300f));
                    {
                        foreach (Card card in cardLibrary.OrderBy(card => card.CardSetType).ThenBy(card => card.Name))
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
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Deck Settings", EditorStyles.boldLabel);
                    GUILayout.Space(10);

                    GUILayout.Label("Primary Skill");
                    customDeck.PrimarySkill = (Enumerators.OverlordSkill) EditorGUILayout.EnumPopup(customDeck.PrimarySkill);

                    GUILayout.Label("Secondary Skill");
                    customDeck.SecondarySkill = (Enumerators.OverlordSkill) EditorGUILayout.EnumPopup(customDeck.SecondarySkill);

                    GUILayout.Label("Hero Id");
                    string heroIdString = GUILayout.TextField(customDeck.HeroId.ToString());
                    if (int.TryParse(heroIdString, out int newHeroId))
                    {
                        customDeck.HeroId = newHeroId;
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCustomDeckCard(DeckCardData deckCard, out bool isRemoved)
        {
            isRemoved = false;
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(_cardNameToDescription[deckCard.CardName]);
                GUILayout.FlexibleSpace();
                string amountString = EditorGUILayout.TextField(deckCard.Amount.ToString(), GUILayout.Width(50));
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

                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    isRemoved = true;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
