using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class CheatUI : MonoBehaviour
    {
        private const string PersistentFileName = "CheatsSettings.json";

        private readonly List<Action> _updateBinds = new List<Action>();

        private IPvPManager _pvpManager;
        private IDataManager _dataManager;
        private CustomDeckEditor _customDeckEditor;

        private void OnGUI()
        {
            UpdateGUIScale();

            GUILayout.BeginArea(new Rect(20, 20, 200, 200), "Cheats", GUI.skin.window);
            {
                _pvpManager.DebugCheats.Enabled = GUILayout.Toggle(_pvpManager.DebugCheats.Enabled, "Enabled");

                GUI.enabled = _pvpManager.DebugCheats.Enabled;
                _pvpManager.DebugCheats.DisableDeckShuffle = GUILayout.Toggle(_pvpManager.DebugCheats.DisableDeckShuffle, "Disable Deck Shuffle");
                _pvpManager.DebugCheats.IgnoreGooRequirements = GUILayout.Toggle(_pvpManager.DebugCheats.IgnoreGooRequirements, "Ignore Goo Requirements");

                GUILayout.BeginHorizontal();
                {
                    _pvpManager.DebugCheats.UseCustomDeck = GUILayout.Toggle(_pvpManager.DebugCheats.UseCustomDeck, "Use Custom Deck");
                    if (GUILayout.Button("Edit"))
                    {
                        if (_customDeckEditor == null)
                        {
                            _customDeckEditor = new CustomDeckEditor(this);
                        }

                        _customDeckEditor.Show();
                    }
                }
                GUILayout.EndHorizontal();

                GUI.enabled = true;
            }
            GUILayout.EndArea();

            _customDeckEditor?.OnGUI();
        }

        private void Start()
        {
            _pvpManager = GameClient.Get<IPvPManager>();
            _dataManager = GameClient.Get<IDataManager>();

            try
            {
                string persistentDataPath = _dataManager.GetPersistentDataPath(PersistentFileName);
                if (!File.Exists(persistentDataPath))
                    return;

                string json = File.ReadAllText(persistentDataPath);
                CheatsConfigurationModel cheatsConfigurationModel = _dataManager.DeserializeFromJson<CheatsConfigurationModel>(json);
                _pvpManager.DebugCheats.CopyFrom(cheatsConfigurationModel.DebugCheatsConfiguration);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private void OnDestroy()
        {
            CheatsConfigurationModel cheatsConfigurationModel = new CheatsConfigurationModel();
            cheatsConfigurationModel.DebugCheatsConfiguration = _pvpManager.DebugCheats;

            string json = _dataManager.SerializeToJson(cheatsConfigurationModel, true);
            File.WriteAllText(_dataManager.GetPersistentDataPath(PersistentFileName), json);
        }

        private void Update()
        {
            foreach (Action updateBind in _updateBinds)
            {
                updateBind();
            }
        }

        private static void UpdateGUIScale() {
            float scaleFactor = UIScaleFactor;

            Vector3 scale;
            scale.x = scaleFactor;
            scale.y = scaleFactor;
            scale.z = 1f;

            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
        }

        private static float UIScaleFactor => Screen.dpi / 96f;

        private class CustomDeckEditor
        {
            private readonly CheatUI _cheatUI;
            private readonly IMGUIPopup _primarySkillPopup = new IMGUIPopup();
            private readonly IMGUIPopup _secondarySkillPopup = new IMGUIPopup();
            private readonly Dictionary<string, string> _cardNameToDescription = new Dictionary<string, string>();
            private bool _visible;
            private Vector2 _customDeckScrollPosition;
            private Vector2 _cardLibraryScrollPosition;

            public CustomDeckEditor(CheatUI cheatUI)
            {
                _cheatUI = cheatUI;
            }

            public void Show()
            {
                _visible = true;
                CardsLibraryData cardsLibraryData = _cheatUI._dataManager.CachedCardsLibraryData;
                if (_cheatUI._pvpManager.DebugCheats.CustomDeck == null)
                {
                    _cheatUI._pvpManager.DebugCheats.CustomDeck =
                        new Deck(-1, 0, "custom deck", new List<DeckCardData>(), Enumerators.OverlordSkill.NONE, Enumerators.OverlordSkill.NONE);
                }

                foreach (Card card in cardsLibraryData.Cards.OrderBy(card => card.CardSetType).ThenBy(card => card.Name))
                {
                    _cardNameToDescription[card.Name] = $"{card.Name} (set: {card.CardSetType}, cost: {card.Cost}, atk: {card.Damage}, def: {card.Health})";
                }
            }

            public void Close()
            {
                _visible = false;
            }

            public void OnGUI()
            {
                if (!_visible)
                    return;

                Deck customDeck = _cheatUI._pvpManager.DebugCheats.CustomDeck;
                const float width = 600;
                Rect addedCardsRect = new Rect(Screen.width / 2f / UIScaleFactor - width / 2f, 0f, width, Screen.height / 2f / UIScaleFactor);
                GUI.Label(addedCardsRect, "", GUI.skin.window);
                GUI.Label(addedCardsRect, "", GUI.skin.window);
                GUI.Label(addedCardsRect, "", GUI.skin.window);
                GUILayout.BeginArea(addedCardsRect, "Custom Deck", GUI.skin.window);
                {
                    _customDeckScrollPosition = GUILayout.BeginScrollView(_customDeckScrollPosition);
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
                    GUILayout.EndScrollView();

                    GUILayout.BeginHorizontal();
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
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndArea();

                Rect cardLibraryRect = new Rect(Screen.width / 2f / UIScaleFactor - width / 2f, Screen.height / 2f / UIScaleFactor, width, Screen.height / 2f / UIScaleFactor);
                GUI.Label(cardLibraryRect, "", GUI.skin.window);
                GUI.Label(cardLibraryRect, "", GUI.skin.window);
                GUI.Label(cardLibraryRect, "", GUI.skin.window);
                GUILayout.BeginArea(cardLibraryRect, "Card Library", GUI.skin.window);
                {
                    _cardLibraryScrollPosition = GUILayout.BeginScrollView(_cardLibraryScrollPosition);
                    {
                        CardsLibraryData cardLibrary = _cheatUI._dataManager.CachedCardsLibraryData;
                        foreach (Card card in cardLibrary.Cards)
                        {
                            GUILayout.BeginHorizontal();
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
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndArea();

                Rect auxRect = new Rect(addedCardsRect.xMax, 0f, 200, Screen.height / UIScaleFactor);
                GUI.Label(auxRect, "", GUI.skin.window);
                GUI.Label(auxRect, "", GUI.skin.window);
                GUI.Label(auxRect, "", GUI.skin.window);
                GUILayout.BeginArea(auxRect, "Custom Deck Settings", GUI.skin.window);
                {
                    GUILayout.Space(10);

                    GUILayout.Label("Primary Skill");
                    customDeck.PrimarySkill = DrawEnumPopup(customDeck.PrimarySkill, _primarySkillPopup);

                    GUILayout.Label("Secondary Skill");
                    customDeck.SecondarySkill = DrawEnumPopup(customDeck.SecondarySkill, _secondarySkillPopup);

                    GUILayout.Label("Hero Id");
                    string heroIdString = GUILayout.TextField(customDeck.HeroId.ToString());
                    if (int.TryParse(heroIdString, out int newHeroId))
                    {
                        customDeck.HeroId = newHeroId;
                    }
                }
                GUILayout.EndArea();
            }

            private static T DrawEnumPopup<T>(T value, IMGUIPopup popup)
            {
                T[] values = (T[]) Enum.GetValues(typeof(Enumerators.OverlordSkill));
                for (int i = 0; i < values.Length; i++)
                {
                    if (value.Equals(values[i]))
                    {
                        popup.SelectedItemIndex = i;
                        break;
                    }
                }

                Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.button);
                int selectedIndex = popup.List(
                    rect,
                    values.Select(v => new GUIContent(v.ToString())).ToArray(),
                    GUI.skin.button,
                    GUI.skin.button
                );

                return values[selectedIndex];
            }

            private void DrawCustomDeckCard(DeckCardData deckCard, out bool isRemoved)
            {
                isRemoved = false;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(_cardNameToDescription[deckCard.CardName]);
                    GUILayout.FlexibleSpace();
                    string amountString = GUILayout.TextField(deckCard.Amount.ToString(), GUILayout.Width(50));
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
                GUILayout.EndHorizontal();
            }
        }


        private class CheatsConfigurationModel
        {
            public DebugCheatsConfiguration DebugCheatsConfiguration;
        }
    }
}
