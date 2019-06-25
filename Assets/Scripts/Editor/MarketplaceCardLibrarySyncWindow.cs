using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using JsonConvert = Newtonsoft.Json.JsonConvert;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using JsonReader = Newtonsoft.Json.JsonReader;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using JsonSerializerSettings = Newtonsoft.Json.JsonSerializerSettings;
using JsonWriter = Newtonsoft.Json.JsonWriter;

namespace Loom.ZombieBattleground.Editor
{
    public class MarketplaceCardLibrarySyncWindow : EditorWindow
    {
        private const string CardFaucetCardDetailJsonFilePathPrefsKey = "MarketplaceCardLibrarySyncWindow_cardFaucetCardDetailJsonFilePath";
        private const string CardFaucetCardDetailTronJsonFilePathPrefsKey = "MarketplaceCardLibrarySyncWindow_cardFaucetCardDetailTronJsonFilePath";
        private const string CardFaucetCardDetailBinanceJsonFilePathPrefsKey = "MarketplaceCardLibrarySyncWindow_cardFaucetCardDetailBinanceJsonFilePath";
        private const string GamechainCardLibraryJsonFilePathPrefsKey = "MarketplaceCardLibrarySyncWindow_gamechainCardLibraryJsonFilePath";

        private readonly JsonSerializerSettings _jsonSerializerSettings =
            JsonUtility.CreateStrictSerializerSettings((sender, args) => Debug.LogException(args.ErrorContext.Error));

        private readonly long[] IgnoredCardMouldIds = {
            //900,
            //9001
        };

        private string _cardFaucetCardDetailJsonFilePath;
        private string _cardFaucetCardDetailTronJsonFilePath;
        private string _cardFaucetCardDetailBinanceJsonFilePath;
        private string _gamechainCardLibraryJsonFilePath;

        private Vector2 _scrollPosition;
        private ComparisonResult _comparisonResult;

        [NonSerialized]
        private GamechainCardLibraryRoot _gamechainCardLibrary;

        [NonSerialized]
        private List<CardKey> _cardFaucetCardKeys;

        [NonSerialized]
        private List<CardKey> _cardFaucetTronCardKeys;

        [NonSerialized]
        private List<CardKey> _cardFaucetBinanceCardKeys;

        private void OnGUI()
        {
            using (EditorGUILayout.ScrollViewScope scrollScope = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollScope.scrollPosition;
                DrawMainGui();
            }
        }

        private void DrawMainGui()
        {
            _cardFaucetCardDetailJsonFilePath =
                HandleJsonFileField(
                    _cardFaucetCardDetailJsonFilePath,
                    "CardFaucet 'cardDetail.json' File Path: ",
                    "Select CardFaucet 'cardDetail.json' File",
                    CardFaucetCardDetailJsonFilePathPrefsKey
                );
            
            _cardFaucetCardDetailTronJsonFilePath =
                HandleJsonFileField(
                    _cardFaucetCardDetailTronJsonFilePath,
                    "CardFaucet Tron 'cardDetailTron.json' File Path: ",
                    "Select CardFaucet Tron 'cardDetail.json' File",
                    CardFaucetCardDetailTronJsonFilePathPrefsKey
                );
            
            _cardFaucetCardDetailBinanceJsonFilePath =
                HandleJsonFileField(
                    _cardFaucetCardDetailBinanceJsonFilePath,
                    "CardFaucet Binance 'cardDetailBinance.json' File Path: ",
                    "Select CardFaucet Binance 'cardDetail.json' File",
                    CardFaucetCardDetailBinanceJsonFilePathPrefsKey
                );

            _gamechainCardLibraryJsonFilePath =
                HandleJsonFileField(
                    _gamechainCardLibraryJsonFilePath,
                    "zb_card_meta_data 'card_library.json' File Path: ",
                    "Select zb_card_meta_data 'card_library.json' File",
                    GamechainCardLibraryJsonFilePathPrefsKey
                );

            if (GUILayout.Button("Load And Compare"))
            {
                DoLoadAndCompare();
            }

            if (_comparisonResult == null)
                return;

            DrawComparisonGui();
        }

        private void DrawComparisonGui()
        {
            GUIStyle boldTextStyle = new GUIStyle(GUI.skin.label);
            boldTextStyle.fontStyle = FontStyle.Bold;

            GUILayout.Space(15);

            if (GUILayout.Button("Copy Cards Missing in zb_card_meta_data 'card_library.json' to Clipboard"))
            {
                List<DummyCardWithCardKey> dummyCards =
                    _comparisonResult.CardsMissingOnGamechain
                        .Select(key => new DummyCardWithCardKey
                        {
                            CardKey = key
                        })
                        .ToList();

                string deltaJson = JsonConvert.SerializeObject(dummyCards, Formatting.Indented, _jsonSerializerSettings);
                EditorGUIUtility.systemCopyBuffer = deltaJson;
                ShowNotification(new GUIContent("Done!"));
            }

            EditorGUILayout.LabelField($"Cards Missing in zb_card_meta_data 'card_library.json' ({_comparisonResult.CardsMissingOnGamechain.Count})", boldTextStyle);
            for (int i = 0; i < _comparisonResult.CardsMissingOnGamechain.Count; i++)
            {
                CardKey missingCardKey = _comparisonResult.CardsMissingOnGamechain[i];
                EditorGUILayout.LabelField($"{i + 1,3}. " + missingCardKey);
            }

            GUILayout.Space(15);
            EditorGUILayout.LabelField($"Cards Missing in Marketplace card lists ({_comparisonResult.CardsMissingOnGamechain.Count})", boldTextStyle);
            for (int i = 0; i < _comparisonResult.CardsMissingOnGamechain.Count; i++)
            {
                CardKey missingCardKey = _comparisonResult.CardsMissingOnMarketplace[i];
                EditorGUILayout.LabelField($"{i + 1,3}. " + missingCardKey);
            }
        }

        private void DoLoadAndCompare()
        {
            try
            {
                string cardFaucetCardDetailJson = File.ReadAllText(_cardFaucetCardDetailJsonFilePath);
                string cardFaucetCardDetailTronJson = File.ReadAllText(_cardFaucetCardDetailTronJsonFilePath);
                string cardFaucetCardDetailBinanceJson = File.ReadAllText(_cardFaucetCardDetailBinanceJsonFilePath);
                string gamechainCardLibraryJson = File.ReadAllText(_gamechainCardLibraryJsonFilePath);

                _gamechainCardLibrary = JsonConvert.DeserializeObject<GamechainCardLibraryRoot>(gamechainCardLibraryJson, _jsonSerializerSettings);
                _cardFaucetCardKeys = ParseCardFaucetCardDetails(cardFaucetCardDetailJson);
                _cardFaucetTronCardKeys = ParseCardFaucetCardDetails(cardFaucetCardDetailTronJson);
                _cardFaucetBinanceCardKeys = ParseCardFaucetCardDetails(cardFaucetCardDetailBinanceJson);

                _comparisonResult = new ComparisonResult();
                List<CardKey> combinedCardFaucetCardKeys =
                    _cardFaucetCardKeys
                        .Concat(_cardFaucetTronCardKeys)
                        .Concat(_cardFaucetBinanceCardKeys)
                        .Distinct()
                        .ToList();

                List<CardKey> gamechainCardKeys = _gamechainCardLibrary.Cards.Select(card => card.CardKey).ToList();

                _comparisonResult.CardsMissingOnGamechain =
                    combinedCardFaucetCardKeys
                        .Except(gamechainCardKeys)
                        .OrderBy(key => key.MouldId)
                        .ThenBy(key => key.Variant)
                        .Where(key => !IgnoredCardMouldIds.Contains(key.MouldId.Id))
                        .ToList();

                _comparisonResult.CardsMissingOnMarketplace =
                    gamechainCardKeys
                        .Except(combinedCardFaucetCardKeys)
                        .OrderBy(key => key.MouldId)
                        .ThenBy(key => key.Variant)
                        .Where(key => !IgnoredCardMouldIds.Contains(key.MouldId.Id))
                        .ToList();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError(e.Message);
                ShowNotification(new GUIContent("Error: " + e.Message));
            }
        }

        private string HandleJsonFileField(string path, string label, string openFileTitle, string prefsKey)
        {
            string originalPath = path;
            
            if (String.IsNullOrEmpty(path))
            {
                path = EditorPrefs.GetString(prefsKey);
            }
            
            EditorGUIUtility.labelWidth = 330;

            EditorGUILayout.BeginHorizontal();
            {
                path = EditorGUILayout.TextField(label, path);
                if (GUILayout.Button("Select...", GUILayout.Width(100)))
                {
                    path = UnityEditor.EditorUtility.OpenFilePanel(openFileTitle, Application.dataPath, "json");
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = 0;
            if (path != originalPath)
            {
                EditorPrefs.SetString(prefsKey, path);
            }

            return path;
        }

        private List<CardKey> ParseCardFaucetCardDetails(string json)
        {
            List<CardKey> cardKeys = new List<CardKey>();
            Dictionary<string, CardFaucetCard> cards = JsonConvert.DeserializeObject<Dictionary<string, CardFaucetCard>>(json, _jsonSerializerSettings);
            foreach (CardFaucetCard card in cards.Values)
            {
                cardKeys.Add(new CardKey(new MouldId(card.MouldId), card.Variant));
            }

            return cardKeys;
        }

        private class ComparisonResult
        {
            public List<CardKey> CardsMissingOnGamechain;
            public List<CardKey> CardsMissingOnMarketplace;
        }

        private class DummyCardWithCardKey
        {
            [JsonProperty("cardKey")]
            public CardKey CardKey;
        }

        private class CardFaucetCard : IEquatable<CardFaucetCard>
        {
            [JsonProperty("mouldId")]
            public long MouldId;

            [JsonConverter(typeof(CardFaucetVariantJsonConverter))]
            [JsonProperty("variant")]
            public Enumerators.CardVariant Variant;

            public CardFaucetCard(long mouldId, Enumerators.CardVariant variant)
            {
                MouldId = mouldId;
                Variant = variant;
            }

            public bool Equals(CardFaucetCard other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return MouldId == other.MouldId && Variant == other.Variant;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((CardFaucetCard) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (MouldId.GetHashCode() * 397) ^ (int) Variant;
                }
            }
        }

        private class GamechainCardLibraryRoot
        {
            [JsonProperty("cards")]
            public List<Card> Cards;
        }

        private class CardFaucetVariantJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Enumerators.CardVariant variant = (Enumerators.CardVariant) value;
                string variantString;
                switch (variant)
                {
                    case Enumerators.CardVariant.Standard:
                        variantString = "standard-edition";
                        break;
                    case Enumerators.CardVariant.Backer:
                        variantString = "backers-edition";
                        break;
                    case Enumerators.CardVariant.Limited:
                        variantString = "limited-edition";
                        break;
                    case Enumerators.CardVariant.Binance:
                        variantString = "binance-edition";
                        break;
                    case Enumerators.CardVariant.Tron:
                        variantString = "tron-edition";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                writer.WriteValue(variantString);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                string variantString = reader.Value.ToString();
                switch (variantString)
                {
                    case "standard-edition":
                        return Enumerators.CardVariant.Standard;
                    case "backers-edition":
                        return Enumerators.CardVariant.Backer;
                    case "limited-edition":
                        return Enumerators.CardVariant.Limited;
                    case "binance-edition":
                        return Enumerators.CardVariant.Binance;
                    case "tron-edition":
                        return Enumerators.CardVariant.Tron;
                    default:
                        throw new ArgumentOutOfRangeException(variantString);
                }
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Enumerators.CardVariant);
            }
        }

        [MenuItem("Utility/Data/Open Marketplace Card Library Sync Window")]
        private static void ShowWindow()
        {
            MarketplaceCardLibrarySyncWindow window = GetWindow<MarketplaceCardLibrarySyncWindow>();
            window.titleContent = new GUIContent("Marketplace Card Library Sync");
        }
    }
}
