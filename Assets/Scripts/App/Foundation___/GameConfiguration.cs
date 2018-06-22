// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.IO;
using System.Linq;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;
using UnityEngine.Assertions;

using FullSerializer;

namespace CCGKit
{
    /// <summary>
    /// Contains the entire game configuration details, which are comprised of general game settings,
    /// player/card/effect definitions and the card database.
    /// </summary>
    public class GameConfiguration
    {
        /// <summary>
        /// The properties of the game.
        /// </summary>
        public GameProperties properties = new GameProperties();

        /// <summary>
        /// The game zones of the game.
        /// </summary>
        public List<GameZoneType> gameZones = new List<GameZoneType>();

        /// <summary>
        /// The player stats.
        /// </summary>
        public List<DefinitionStat> playerStats = new List<DefinitionStat>();

        /// <summary>
        /// The card types.
        /// </summary>
        public List<CardType> cardTypes = new List<CardType>();

        /// <summary>
        /// The keywords of the game.
        /// </summary>
        public List<Keyword> keywords = new List<Keyword>();

        /// <summary>
        /// The card sets of the game.
        /// </summary>
        public List<CardSet> cardSets = new List<CardSet>();

        /// <summary>
        /// The cards of the game.
        /// </summary>
        public List<Card> cards = new List<Card>();

        /// <summary>
        /// The JSON serializer.
        /// </summary>
        private fsSerializer serializer = new fsSerializer();

        /// <summary>
        /// Loads the game configuration from the specified path.
        /// </summary>
        /// <param name="path">The path to the game configuration.</param>
        public void LoadGameConfiguration(string path)
        {
            var gamePropertiesPath = path + "/game_properties.json";
            var gameProperties = LoadJSONFile<GameProperties>(gamePropertiesPath);
            if (gameProperties != null)
            {
                properties = gameProperties;
            }

            var gameZonesPath = path + "/game_zones.json";
            var zones = LoadJSONFile<List<GameZoneType>>(gameZonesPath);
            if (zones != null)
            {
                gameZones = zones;
            }
            if (gameZones.Count > 0)
            {
                GameZoneType.currentId = gameZones.Max(x => x.id) + 1;
            }

            var playerStatsPath = path + "/player_stats.json";
            var stats = LoadJSONFile<List<DefinitionStat>>(playerStatsPath);
            if (stats != null)
            {
                playerStats = stats;
            }
            if (playerStats.Count > 0)
            {
                PlayerStat.currentId = playerStats.Max(x => x.id) + 1;
            }

            var cardTypesPath = path + "/card_types.json";
            var types = LoadJSONFile<List<CardType>>(cardTypesPath);
            if (types != null)
            {
                cardTypes = types;
            }
            if (cardTypes.Count > 0)
            {
                CardType.currentId = cardTypes.Max(x => x.id) + 1;
            }
            var ids = new List<int>();
            foreach (var type in cardTypes)
            {
                if (type.stats.Count > 0)
                {
                    ids.Add(type.stats.Max(x => x.id));
                }
            }
            if (ids.Count > 0)
            {
                CardStat.currentId = ids.Max() + 1;
            }

            var keywordsPath = path + "/keywords.json";
            var keywords = LoadJSONFile<List<Keyword>>(keywordsPath);
            if (keywords != null)
            {
                this.keywords = keywords;
            }
            if (this.keywords.Count > 0)
            {
                Keyword.currentId = this.keywords.Max(x => x.id) + 1;
            }

            var cardLibraryPath = path + "/card_library.json";
            var cardLibrary = LoadJSONFile<List<CardSet>>(cardLibraryPath);
            if (cardLibrary != null)
            {
                cardSets = cardLibrary;
            }
            var max = -1;
            foreach (var set in cardSets)
            {
                var currentMax = set.cards.Max(x => x.id);
                if (currentMax > max)
                {
                    max = currentMax;
                }
            }
            Card.currentId = max + 1;
        }

        /// <summary>
        /// Loads the game configuration at runtime.
        /// </summary>
        public void LoadGameConfigurationAtRuntime()
        {
            var gamePropertiesJSON = Resources.Load<TextAsset>("game_properties");
            Assert.IsTrue(gamePropertiesJSON != null);
            var gameProperties = LoadJSONString<GameProperties>(gamePropertiesJSON.text);
            if (gameProperties != null)
            {
                properties = gameProperties;
            }

            var gameZonesJSON = Resources.Load<TextAsset>("game_zones");
            Assert.IsTrue(gameZonesJSON != null);
            var zones = LoadJSONString<List<GameZoneType>>(gameZonesJSON.text);
            if (zones != null)
            {
                gameZones = zones;
            }

            var playerStatsJSON = Resources.Load<TextAsset>("player_stats");
            Assert.IsTrue(playerStatsJSON != null);
            var stats = LoadJSONString<List<DefinitionStat>>(playerStatsJSON.text);
            if (stats != null)
            {
                playerStats = stats;
            }

            var cardTypesJSON = Resources.Load<TextAsset>("card_types");
            Assert.IsTrue(cardTypesJSON != null);
            var types = LoadJSONString<List<CardType>>(cardTypesJSON.text);
            if (types != null)
            {
                cardTypes = types;
            }

            var keywordsJSON = Resources.Load<TextAsset>("keywords");
            Assert.IsTrue(keywordsJSON != null);
            var keywords = LoadJSONString<List<Keyword>>(keywordsJSON.text);
            if (keywords != null)
            {
                this.keywords = keywords;
            }

            var cardLibraryJSON = Resources.Load<TextAsset>("card_library");
            Assert.IsTrue(cardLibraryJSON != null);
            var cardLibrary = LoadJSONString<List<CardSet>>(cardLibraryJSON.text);
            if (cardLibrary != null)
            {
                cardSets = cardLibrary;
                foreach (var set in cardSets)
                {
                    foreach (var card in set.cards)
                    {
                        cards.Add(card);
                    }
                }
            }
        }

        /// <summary>
        /// Loads the JSON file from the specified path.
        /// </summary>
        /// <typeparam name="T">The type of data to load.</typeparam>
        /// <param name="path">The path to the file.</param>
        /// <returns>The data contained in the file.</returns>
        private T LoadJSONFile<T>(string path) where T : class
        {
            if (File.Exists(path))
            {
                var file = new StreamReader(path);
                var fileContents = file.ReadToEnd();
                var data = fsJsonParser.Parse(fileContents);
                object deserialized = null;
                serializer.TryDeserialize(data, typeof(T), ref deserialized).AssertSuccessWithoutWarnings();
                file.Close();
                return deserialized as T;
            }
            return null;
        }

        /// <summary>
        /// Loads the JSON data from the specified string.
        /// </summary>
        /// <typeparam name="T">The type of data to load.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <returns>The data contained in the string.</returns>
        private T LoadJSONString<T>(string json) where T : class
        {
            var data = fsJsonParser.Parse(json);
            object deserialized = null;
            serializer.TryDeserialize(data, typeof(T), ref deserialized).AssertSuccessWithoutWarnings();
            return deserialized as T;
        }

        /// <summary>
        /// Saves the game configuration to the specified path.
        /// </summary>
        /// <param name="path">The path to the game configuration.</param>
        public void SaveGameConfiguration(string path)
        {
#if UNITY_EDITOR
            SaveJSONFile(path + "/game_properties.json", properties);
            SaveJSONFile(path + "/game_zones.json", gameZones);
            SaveJSONFile(path + "/player_stats.json", playerStats);
            SaveJSONFile(path + "/card_types.json", cardTypes);
            SaveJSONFile(path + "/keywords.json", keywords);
            SaveJSONFile(path + "/card_library.json", cardSets);
            AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// Saves the game configuration to the path selected by the user.
        /// </summary>
        public void SaveGameConfigurationAs()
        {
#if UNITY_EDITOR
            var path = EditorUtility.OpenFolderPanel("Select game configuration folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                EditorPrefs.SetString("GameConfigurationPath", path);
                SaveGameConfiguration(path);
            }
#endif
        }

        /// <summary>
        /// Saves the specified data to the specified path.
        /// </summary>
        /// <typeparam name="T">The type of data to save.</typeparam>
        /// <param name="path">The path where to save the data.</param>
        /// <param name="data">The data to save.</param>
        private void SaveJSONFile<T>(string path, T data) where T : class
        {
            fsData serializedData;
            serializer.TrySerialize(data, out serializedData).AssertSuccessWithoutWarnings();
            var file = new StreamWriter(path);
            var json = fsJsonPrinter.PrettyJson(serializedData);
            file.WriteLine(json);
            file.Close();
        }

        /// <summary>
        /// Returns the card with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the card.</param>
        /// <returns>The card with the specified identifier.</returns>
        public Card GetCard(int id)
        {
            var libraryCard = cards.Find(x => x.id == id);
            return libraryCard;
        }

        /// <summary>
        /// Returns the number of cards in the configuration.
        /// </summary>
        /// <returns>The number of cards in the configuration.</returns>
        public int GetNumCards()
        {
            return cards.Count;
        }
    }
}
