#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEditor;
using UnityEngine;
using CardList = Loom.ZombieBattleground.Data.CardList;


namespace Loom.ZombieBattleground.Helpers.Tools
{
    public class CardLibraryEditTool : EditorWindow
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture,
                Converters = {
                        new StringEnumConverter()
                },
                CheckAdditionalContent = true,
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = (sender, args) =>
                {
                    Debug.LogException(args.ErrorContext.Error);
                }
            };


        private CardList _currentWorkingCardsLibrary;

        private string _importedCollectionPath;

        private bool _isCardsImported;

        private int _selectedCardIndex = 0;

        private Data.Card _selectedCard;

        private bool _isCardSelected = false;

        [MenuItem("Window/ZombieBattleground/Card Library Edit Tool")]
        static void Init()
        {
            CardLibraryEditTool window = (CardLibraryEditTool)EditorWindow.GetWindow(typeof(CardLibraryEditTool));
            window.Show();
        }

        private void OnGUI()
        {
            DrawCardLibraryImporting();
            DrawCardsDropdown();
            DrawSelectedCard();
        }


        private void DrawChoosingCard()
        {

        }

        private void DrawCardLibraryImporting()
        {
            GUILayout.Label("Importing Card Library", EditorStyles.boldLabel);

            GUILayout.Space(5);

            if (EditorGUILayout.DropdownButton(new GUIContent("Import", "import file"), FocusType.Passive))
            {
                _importedCollectionPath = EditorUtility.OpenFilePanel("Select card collection", "", "json");
                if (_importedCollectionPath.Length != 0)
                {
                    _currentWorkingCardsLibrary = JsonConvert.DeserializeObject<CardList>(File.ReadAllText(_importedCollectionPath), JsonSerializerSettings);
                    _isCardsImported = true;
                }
                else
                {
                    _currentWorkingCardsLibrary = null;
                    _isCardsImported = false;
                }
            }

            if (_isCardsImported)
            {
                GUILayout.Space(5);
                GUILayout.Label("Imported: " + _importedCollectionPath, EditorStyles.label);
            }

            GUILayout.Space(10);
        }

        private void DrawCardsDropdown()
        {
            if (_isCardsImported)
            {
                GUILayout.Label("Selecting Card", EditorStyles.boldLabel);

                string[] options = _currentWorkingCardsLibrary.Cards.Select(x => _currentWorkingCardsLibrary.Cards.IndexOf(x) + ": " + x.Name).ToArray();

                _selectedCardIndex = EditorGUILayout.Popup("Card: ", _selectedCardIndex, options);

                _selectedCard = _currentWorkingCardsLibrary.Cards[_selectedCardIndex];

                _isCardSelected = true;

                GUILayout.Space(10);
            }
            else
            {
                _isCardSelected = false;
            }
        }

        private void DrawSelectedCard()
        {
            if (_isCardSelected)
            {
                GUILayout.Label("Abilities: ", EditorStyles.boldLabel);

                string[] vfxTypes = Enum.GetNames(typeof(Enumerators.VisualEffectType));
                int indexOfVfxType = 0;

                foreach (var abilityInfo in _selectedCard.Abilities)
                {
                    GUILayout.Label(abilityInfo.AbilityType.ToString(), EditorStyles.miniBoldLabel);

                    GUILayout.Label("VisualEffectsToPlay", EditorStyles.label);
                    foreach (var vfxInfo in abilityInfo.VisualEffectsToPlay)
                    {
                        indexOfVfxType = vfxTypes.ToList().IndexOf(vfxInfo.Type.ToString());
                        indexOfVfxType = EditorGUILayout.Popup("Type: ", indexOfVfxType, vfxTypes);

                        // vfxInfo.Type = Utilites.CastStringTuEnum<Enumerators.VisualEffectType>(vfxTypes[indexOfVfxType], true);

                        // vfxInfo.Path =
                        GUILayout.TextField(vfxInfo.Path, EditorStyles.textField);

                        GUILayout.Space(5);
                    }
                }
            }
        }

        /*
        public class CardLibrary
        {
            public List<CardSet> Sets;
        }

        public class CardSet
        {
            public Enumerators.SetType Name;
            public List<Card> Cards;
        }

        public class Card
        {
            public long Id;

            public string Name;

            public int Cost;

            public string Description;

            public string FlavorText;

            public string Picture;

            public int Damage;

            public int Health;

            [JsonProperty("Set")]
            public Enumerators.SetType CardSetType;

            public string Frame;

            [JsonProperty("Kind")]
            public Enumerators.CardKind CardKind;

            [JsonProperty("Rank")]
            public Enumerators.CardRank CardRank;

            [JsonProperty("Type")]
            public Enumerators.CardType CardType;

            public List<AbilityData> Abilities;

            public CardViewInfo CardViewInfo;

            public Enumerators.UniqueAnimationType UniqueAnimationType;
        }

        public class CardViewInfo
        {
            public FloatVector3 Position { get; protected set; } = FloatVector3.Zero;
            public FloatVector3 Scale { get; protected set; } = new FloatVector3(0.38f);
        }

        public class VisualEffectToPlay
        {
            public Enumerators.VisualEffectType Type;
            public string Path;
        }

        public class AbilityData
        {
            [JsonProperty("Type")]
            public Enumerators.AbilityType AbilityType { get; private set; }

            [JsonProperty("activity_type")]
            public Enumerators.AbilityActivityType ActivityType { get; private set; }

            [JsonProperty("call_type")]
            public Enumerators.AbilityCallType CallType { get; private set; }

            [JsonProperty("target_type")]
            public List<Enumerators.AbilityTargetType> AbilityTargetTypes { get; private set; }

            [JsonProperty("stat_type")]
            public Enumerators.StatType AbilityStatType { get; private set; }

            [JsonProperty("set_type")]
            public Enumerators.SetType AbilitySetType { get; private set; }

            [JsonProperty("effect_type")]
            public Enumerators.AbilityEffectType AbilityEffectType { get; private set; }

            [JsonProperty("attack_restriction")]
            public Enumerators.AttackRestriction AttackRestriction { get; private set; }

            [JsonProperty("card_type")]
            public Enumerators.CardType TargetCardType { get; private set; }

            [JsonProperty("unit_status")]
            public Enumerators.UnitStatusType TargetUnitStatusType { get; private set; }

            [JsonProperty("unit_type")]
            public Enumerators.CardType TargetUnitType { get; private set; }

            public int Value { get; private set; }

            public int Damage { get; private set; }

            public int Health { get; private set; }

            public string Name { get; private set; }

            public int Turns { get; private set; }

            public int Count { get; private set; }

            public int Delay { get; private set; }

            public List<VisualEffectToPlay> VisualEffectsToPlay;

            [JsonProperty("mechanicDescriptionType")]
            public Enumerators.GameMechanicDescriptionType GameMechanicDescriptionType;

            [JsonProperty("target_set")]
            public Enumerators.SetType TargetSetType;

            [JsonProperty("SubTrigger")]
            public Enumerators.AbilitySubTrigger AbilitySubTrigger;

            public List<ChoosableAbility> ChoosableAbilities;

            public int Defense { get; private set; }

            public int Cost { get; private set; }
        }

        public class ChoosableAbility
        {
            public string Description { get; private set; }
            public AbilityData AbilityData { get; private set; }
        } */
    }
}

#endif
