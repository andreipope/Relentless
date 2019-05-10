using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class CheatUI : MonoBehaviour
    {
        private static readonly ILog Log = Logging.GetLog(nameof(CheatUI));

        private const string PersistentFileName = "CheatsSettings.json";

        private readonly List<Action> _updateBinds = new List<Action>();
        private IPvPManager _pvpManager;
        private IDataManager _dataManager;
        private CustomDeckEditor _customDeckEditor;

        private static float UIScaleFactor => Math.Min(2f, Screen.dpi / 96f);

        private void OnGUI()
        {
            GUIUtility.ScaleAroundPivot(Vector2.one * UIScaleFactor, Vector2.zero);

            Rect pvpCheatsRect = new Rect(20, 20, 200, 150);
            GUILayout.BeginArea(pvpCheatsRect, "PvP Cheats", Styles.OpaqueWindow);
            {
                _pvpManager.DebugCheats.Enabled = GUILayout.Toggle(_pvpManager.DebugCheats.Enabled, "Enabled");

                GUI.enabled = _pvpManager.DebugCheats.Enabled;
                _pvpManager.DebugCheats.DisableDeckShuffle = GUILayout.Toggle(_pvpManager.DebugCheats.DisableDeckShuffle, "Disable Deck Shuffle");
                _pvpManager.DebugCheats.IgnoreGooRequirements = GUILayout.Toggle(_pvpManager.DebugCheats.IgnoreGooRequirements, "Ignore Goo Requirements");
                _pvpManager.DebugCheats.SkipMulligan = GUILayout.Toggle(_pvpManager.DebugCheats.SkipMulligan, "Skip Mulligan");

                GUILayout.BeginHorizontal();
                {
                    _pvpManager.DebugCheats.UseCustomDeck = GUILayout.Toggle(_pvpManager.DebugCheats.UseCustomDeck, "Use Custom Deck");
                    if (GUILayout.Button("Edit"))
                    {
                        if (_customDeckEditor == null)
                        {
                            _customDeckEditor = new CustomDeckEditor(this);
                        }

                        if (_customDeckEditor.Visible)
                        {
                            _customDeckEditor.Close();
                        }
                        else
                        {
                            _customDeckEditor.Show();
                        }
                    }
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("Find Match"))
                {
                    Deck deck = _dataManager.CachedDecksData.Decks[0];
                    GameClient.Get<IUIManager>().GetPage<GameplayPage>().CurrentDeckId = deck.Id;
                    GameClient.Get<IGameplayManager>().CurrentPlayerDeck = deck;
                    GameClient.Get<IMatchManager>().MatchType = Enumerators.MatchType.PVP;
                    GameClient.Get<IMatchManager>().FindMatch();
                }

                GUI.enabled = true;
            }
            GUILayout.EndArea();

            pvpCheatsRect.y += pvpCheatsRect.height + 15;
            pvpCheatsRect.height = 75;
            GUILayout.BeginArea(pvpCheatsRect, "PvP Options", Styles.OpaqueWindow);
            {
                _pvpManager.UseBackendGameLogic = GUILayout.Toggle(_pvpManager.UseBackendGameLogic, "Use Backend Logic");
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
                Log.Warn(e);
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

        private class CustomDeckEditor
        {
            private readonly CheatUI _cheatUI;
            private readonly IMGUIPopup _primarySkillPopup = new IMGUIPopup();
            private readonly IMGUIPopup _secondarySkillPopup = new IMGUIPopup();
            private readonly Dictionary<string, string> _cardNameToDescription = new Dictionary<string, string>();
            private bool _visible;
            private Vector2 _customDeckScrollPosition;
            private Vector2 _cardLibraryScrollPosition;
            private string _nameFilterString = "";

            public bool Visible => _visible;

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
                        new Deck(new DeckId(-1), new OverlordId(0), "custom deck", new List<DeckCardData>(), Enumerators.Skill.NONE, Enumerators.Skill.NONE);
                }

                foreach (Card card in cardsLibraryData.Cards)
                {
                    _cardNameToDescription[card.Name] = $"{card.Name} (faction: {card.Faction}, cost: {card.Cost}, atk: {card.Damage}, def: {card.Defense})";
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

                const float customDeckScreenHeightRatio = 1f / 3f;
                Deck customDeck = _cheatUI._pvpManager.DebugCheats.CustomDeck;
                GUILayout.BeginHorizontal(GUILayout.Width(Screen.width / UIScaleFactor));
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginVertical(GUILayout.Width(500f));
                    {
                        // Custom Deck
                        GUILayout.BeginVertical("Custom Deck", Styles.OpaqueWindow, GUILayout.Height(Screen.height * customDeckScreenHeightRatio / UIScaleFactor));
                        {
                            _customDeckScrollPosition = GUILayout.BeginScrollView(_customDeckScrollPosition);
                            {
                                foreach (DeckCardData deckCard in customDeck.Cards)
                                {
                                    DrawCustomDeckCard(customDeck, deckCard);
                                }
                            }
                            GUILayout.EndScrollView();

                            GUILayout.BeginHorizontal();
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
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();

                        // Card Library
                        GUILayout.BeginVertical("Card Library", Styles.OpaqueWindow, GUILayout.Height(Screen.height * (1f - customDeckScreenHeightRatio) / UIScaleFactor));
                        {
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Filter Name ", GUILayout.ExpandWidth(false));
                                _nameFilterString = GUILayout.TextField(_nameFilterString).Trim();
                            }

                            GUILayout.EndHorizontal();
                            _cardLibraryScrollPosition = GUILayout.BeginScrollView(_cardLibraryScrollPosition);
                            {
                                CardsLibraryData cardLibrary = _cheatUI._dataManager.CachedCardsLibraryData;
                                foreach (Card card in cardLibrary.Cards.OrderBy(card => card.Faction).ThenBy(card => card.Name))
                                {
                                    if (!String.IsNullOrWhiteSpace(_nameFilterString) && card.Name.IndexOf(_nameFilterString, StringComparison.InvariantCultureIgnoreCase) == -1)
                                        continue;

                                    GUILayout.BeginHorizontal();
                                    {
                                        GUILayout.Label(_cardNameToDescription[card.Name]);
                                        if (GUILayout.Button("Add", GUILayout.Width(70)))
                                        {
                                            if (!customDeck.Cards.Any(deckCard => deckCard.MouldId == card.MouldId))
                                            {
                                                DeckCardData deckCardData = new DeckCardData(card.MouldId, 0);
                                                customDeck.Cards.Add(deckCardData);
                                            }
                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                            GUILayout.EndScrollView();

                            if (GUILayout.Button("Close"))
                            {
                                Close();
                            }
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical("Deck Properties", Styles.OpaqueWindow, GUILayout.Width(150f));
                    {
                        // Options
                        GUILayout.Space(10);

                        GUILayout.Label("Primary Skill");
                        customDeck.PrimarySkill = DrawEnumPopup(customDeck.PrimarySkill, _primarySkillPopup);

                        GUILayout.Label("Secondary Skill");
                        customDeck.SecondarySkill = DrawEnumPopup(customDeck.SecondarySkill, _secondarySkillPopup);

                        GUILayout.Label("Overlord Id");
                        string overlordIdString = GUILayout.TextField(customDeck.OverlordId.Id.ToString());
                        if (int.TryParse(overlordIdString, out int newOverlordId))
                        {
                            customDeck.OverlordId = new OverlordId(newOverlordId);
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
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

                GUILayout.BeginHorizontal();
                {
                    string cardName = _cheatUI._dataManager.CachedCardsLibraryData.GetCardFromMouldId(deckCard.MouldId).Name;
                    if (!_cardNameToDescription.TryGetValue(cardName, out string cardDescription))
                    {
                        customDeck.Cards.Remove(deckCard);
                        GUIUtility.ExitGUI();
                    }
                    GUILayout.Label(cardDescription);
                    GUILayout.FlexibleSpace();
                    string amountString = GUILayout.TextField(deckCard.Amount.ToString(), GUILayout.Width(35));
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
                GUILayout.EndHorizontal();
            }

            private static T DrawEnumPopup<T>(T value, IMGUIPopup popup)
            {
                T[] values = (T[]) Enum.GetValues(typeof(Enumerators.Skill));
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
        }

        private class CheatsConfigurationModel
        {
            public DebugCheatsConfiguration DebugCheatsConfiguration;
        }

        private static class Styles
        {
            public static GUIStyle OpaqueWindow;

            static Styles()
            {
                OpaqueWindow = new GUIStyle(GUI.skin.window);
                OpaqueWindow.normal.background = Resources.Load<Texture2D>("GUI/skin-window-opaque");
            }
        }
    }
}
