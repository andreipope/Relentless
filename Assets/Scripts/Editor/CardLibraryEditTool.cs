#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEditor;
using UnityEngine;
using CardList = Loom.ZombieBattleground.Data.CardList;
using Logger = Loom.WebSocketSharp.Logger;

namespace Loom.ZombieBattleground.Helpers.Tools
{
    public class CardLibraryEditTool : EditorWindow
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings =
            JsonUtility.CreateStrictSerializerSettings((sender, args) => UnityEngine.Debug.LogException(args.ErrorContext.Error));

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
            DrawExportFunctions();
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
                    _currentWorkingCardsLibrary =
                        JsonConvert.DeserializeObject<CardList>(
                            File.ReadAllText(_importedCollectionPath),
                            _jsonSerializerSettings
                        );
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

                foreach (Data.AbilityData abilityInfo in _selectedCard.Abilities)
                {
                    DrawAbilityConfigurtion(abilityInfo);

                    if (abilityInfo.ChoosableAbilities != null)
                    {
                        foreach (Data.AbilityData choosableAbilityInfo in abilityInfo.ChoosableAbilities.Select(x => x.AbilityData))
                        {
                            DrawAbilityConfigurtion(choosableAbilityInfo, true);
                        }
                    }
                }
            }
        }

        private void DrawAbilityConfigurtion(Data.AbilityData abilityInfo, bool itsAbilityFromChoosable = false)
        {
            if (itsAbilityFromChoosable)
            {
                GUILayout.Label("---This ability located in Choosable Abilities---", EditorStyles.label);
            }

            GUILayout.Label(abilityInfo.Ability.ToString(), EditorStyles.miniBoldLabel);

            GUILayout.Label("VisualEffectsToPlay", EditorStyles.label);


            Data.AbilityData.VisualEffectInfo vfxInfo;
            List<Data.AbilityData.VisualEffectInfo> vfxesToDelete = new List<Data.AbilityData.VisualEffectInfo>();
            for (int i = 0; i < abilityInfo.VisualEffectsToPlay.Count; i++)
            {
                vfxInfo = abilityInfo.VisualEffectsToPlay[i];
                Enumerators.VisualEffectType newVfxTypEnum = (Enumerators.VisualEffectType) EditorGUILayout.EnumPopup("Type: ", vfxInfo.Type);

                vfxInfo = new AbilityData.VisualEffectInfo(newVfxTypEnum, GUILayout.TextField(vfxInfo.Path, EditorStyles.textField));
                abilityInfo.VisualEffectsToPlay[i] = vfxInfo;

                if (EditorGUILayout.DropdownButton(new GUIContent("Delete", "delete vfx"), FocusType.Passive))
                {
                    vfxesToDelete.Add(vfxInfo);
                }

                GUILayout.Space(5);

                GUILayout.Label("----------------------------", EditorStyles.miniLabel);
            }

            if (vfxesToDelete.Count > 0)
            {
                foreach (Data.AbilityData.VisualEffectInfo vfx in vfxesToDelete)
                {
                    abilityInfo.VisualEffectsToPlay.Remove(vfx);
                }
            }

            GUILayout.Space(5);

            if (EditorGUILayout.DropdownButton(new GUIContent("Add New", "add new vfx"), FocusType.Passive))
            {
                abilityInfo.VisualEffectsToPlay.Add(new Data.AbilityData.VisualEffectInfo(Enumerators.VisualEffectType.Impact, string.Empty));
            }
        }

        private void DrawExportFunctions()
        {
            if (_isCardsImported)
            {
                GUILayout.Space(5);

                GUILayout.Label("Exporting Card Library", EditorStyles.boldLabel);

                GUILayout.Space(5);

                if (EditorGUILayout.DropdownButton(new GUIContent("Export", "exporting card library in json"), FocusType.Passive))
                {
                    string pathToFile = EditorUtility.SaveFilePanel("Save card collection", "", "card_library", "json");

                    if (!string.IsNullOrEmpty(pathToFile))
                    {
                        File.WriteAllText(
                            pathToFile,
                            JsonConvert.SerializeObject(
                                _currentWorkingCardsLibrary,
                                Formatting.Indented,
                                _jsonSerializerSettings
                                ));
                        Process.Start(new FileInfo(pathToFile).Directory.FullName);
                    }
                }
            }
        }
    }
}

#endif
