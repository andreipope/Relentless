using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Loom.ZombieBattleground.Editor
{
    [Serializable]
    public class MarketplaceCardLibrarySyncGui
    {
        private const string CardFaucetCardDetailJsonFilePathPrefsKey = "MarketplaceCardLibrarySyncWindow_cardFaucetCardDetailJsonFilePath";
        private const string CardFaucetCardDetailTronJsonFilePathPrefsKey =
            "MarketplaceCardLibrarySyncWindow_cardFaucetCardDetailTronJsonFilePath";
        private const string CardFaucetCardDetailBinanceJsonFilePathPrefsKey =
            "MarketplaceCardLibrarySyncWindow_cardFaucetCardDetailBinanceJsonFilePath";
        private const string GamechainCardLibraryJsonFilePathPrefsKey = "MarketplaceCardLibrarySyncWindow_gamechainCardLibraryJsonFilePath";

        private readonly JsonSerializerSettings _jsonSerializerSettings =
            JsonUtility.CreateStrictSerializerSettings((sender, args) => Debug.LogException(args.ErrorContext.Error));

        private readonly long[] IgnoredCardMouldIds =
        {
            //900,
            //9001
        };

        [SerializeField]
        private EditorWindow _ownerWindow;

        [SerializeField]
        private bool _onlyShowStandardEdition;

        private string _cardFaucetCardDetailJsonFilePath;
        private string _cardFaucetCardDetailTronJsonFilePath;
        private string _cardFaucetCardDetailBinanceJsonFilePath;
        private string _gamechainCardLibraryJsonFilePath;

        private ComparisonResult _comparisonResult;

        [NonSerialized]
        private GamechainCardLibraryRoot _gamechainCardLibrary;

        [NonSerialized]
        private List<CardKey> _cardFaucetCardKeys;

        [NonSerialized]
        private List<CardKey> _cardFaucetTronCardKeys;

        [NonSerialized]
        private List<CardKey> _cardFaucetBinanceCardKeys;

        public MarketplaceCardLibrarySyncGui()
        {
        }

        public MarketplaceCardLibrarySyncGui(EditorWindow ownerWindow)
        {
            _ownerWindow = ownerWindow;
        }

        public void Draw()
        {
            _cardFaucetCardDetailJsonFilePath =
                EditorSpecialGuiUtility.DrawPersistentFilePathField(
                    _cardFaucetCardDetailJsonFilePath,
                    "CardFaucet 'cardDetail.json' File Path: ",
                    "Select CardFaucet 'cardDetail.json' File",
                    "json",
                    CardFaucetCardDetailJsonFilePathPrefsKey
                );

            _cardFaucetCardDetailTronJsonFilePath =
                EditorSpecialGuiUtility.DrawPersistentFilePathField(
                    _cardFaucetCardDetailTronJsonFilePath,
                    "CardFaucet Tron 'cardDetailTron.json' File Path: ",
                    "Select CardFaucet Tron 'cardDetail.json' File",
                    "json",
                    CardFaucetCardDetailTronJsonFilePathPrefsKey
                );

            _cardFaucetCardDetailBinanceJsonFilePath =
                EditorSpecialGuiUtility.DrawPersistentFilePathField(
                    _cardFaucetCardDetailBinanceJsonFilePath,
                    "CardFaucet Binance 'cardDetailBinance.json' File Path: ",
                    "Select CardFaucet Binance 'cardDetail.json' File",
                    "json",
                    CardFaucetCardDetailBinanceJsonFilePathPrefsKey
                );

            _gamechainCardLibraryJsonFilePath =
                EditorSpecialGuiUtility.DrawPersistentFilePathField(
                    _gamechainCardLibraryJsonFilePath,
                    "zb_card_meta_data 'card_library.json' File Path: ",
                    "Select zb_card_meta_data 'card_library.json' File",
                    "json",
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
            GUILayout.Space(15);

            if (GUILayout.Button("Copy Cards Missing in zb_card_meta_data 'card_library.json' to Clipboard"))
            {
                CopyCardKeyListToClipboard(_comparisonResult.CardsMissingOnGamechain);
            }

            if (GUILayout.Button("Copy Card Variants Missing in zb_card_meta_data 'card_library.json' to Clipboard"))
            {
                CopyCardKeyListToClipboard(_comparisonResult.CardVariantsMissing);
            }

            _onlyShowStandardEdition = EditorGUILayout.ToggleLeft("Hide non-Standard edition cards", _onlyShowStandardEdition);

            void DrawCardKeysList(List<CardKey> cardKeys)
            {
                int counter = 1;
                for (int i = 0; i < cardKeys.Count; i++)
                {
                    CardKey cardKey = cardKeys[i];
                    if (_onlyShowStandardEdition && cardKey.Variant != Enumerators.CardVariant.Standard)
                        continue;

                    EditorGUILayout.LabelField($"{counter,3}. " + cardKey);
                    counter++;
                }
            }

            EditorGUILayout.LabelField(
                $"Cards Missing in zb_card_meta_data 'card_library.json' ({_comparisonResult.CardsMissingOnGamechain.Count})",
                EditorStyles.boldLabel);
            DrawCardKeysList(_comparisonResult.CardsMissingOnGamechain);

            GUILayout.Space(15);
            EditorGUILayout.LabelField($"Cards Missing in Marketplace card lists ({_comparisonResult.CardsMissingOnGamechain.Count})", EditorStyles.boldLabel);
            DrawCardKeysList(_comparisonResult.CardsMissingOnMarketplace);

            GUILayout.Space(15);
            EditorGUILayout.LabelField($"Card Variants Missing in zb_card_meta_data 'card_library.json' ({_comparisonResult.CardVariantsMissing.Count})", EditorStyles.boldLabel);
            DrawCardKeysList(_comparisonResult.CardVariantsMissing);
        }

        private void CopyCardKeyListToClipboard(List<CardKey> cardKeys)
        {
            List<DummyCardWithCardKey> dummyCards =
                cardKeys
                    .Select(key => new DummyCardWithCardKey
                    {
                        CardKey = key
                    })
                    .ToList();

            string deltaJson = JsonConvert.SerializeObject(dummyCards, Formatting.Indented, _jsonSerializerSettings);
            EditorGUIUtility.systemCopyBuffer = deltaJson;
            _ownerWindow.ShowNotification(new GUIContent("Done!"));
        }

        private void DoLoadAndCompare()
        {
            try
            {
                string cardFaucetCardDetailJson = File.ReadAllText(_cardFaucetCardDetailJsonFilePath);
                string cardFaucetCardDetailTronJson = File.ReadAllText(_cardFaucetCardDetailTronJsonFilePath);
                string cardFaucetCardDetailBinanceJson = File.ReadAllText(_cardFaucetCardDetailBinanceJsonFilePath);
                string gamechainCardLibraryJson = File.ReadAllText(_gamechainCardLibraryJsonFilePath);

                _gamechainCardLibrary =
                    JsonConvert.DeserializeObject<GamechainCardLibraryRoot>(gamechainCardLibraryJson, _jsonSerializerSettings);
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

                HashSet<CardKey> gamechainCardKeys = new HashSet<CardKey>(
                    _gamechainCardLibrary.Cards.Select(card => card.CardKey)
                );

                _comparisonResult.CardsMissingOnGamechain =
                    combinedCardFaucetCardKeys
                        .Except(gamechainCardKeys)
                        .Where(key => !IgnoredCardMouldIds.Contains(key.MouldId.Id))
                        .OrderBy(key => key, CardKey.Comparer)
                        .ToList();

                _comparisonResult.CardsMissingOnMarketplace =
                    gamechainCardKeys
                        .Except(combinedCardFaucetCardKeys)
                        .Where(key => !IgnoredCardMouldIds.Contains(key.MouldId.Id))
                        .OrderBy(key => key, CardKey.Comparer)
                        .ToList();


                _comparisonResult.CardVariantsMissing = new List<CardKey>();
                Enumerators.CardVariant[] cardVariants = (Enumerators.CardVariant[]) Enum.GetValues(typeof(Enumerators.CardVariant));
                foreach (CardKey cardKey in gamechainCardKeys)
                {
                    if (cardKey.Variant != Enumerators.CardVariant.Standard)
                        continue;

                    foreach (Enumerators.CardVariant variant in cardVariants)
                    {
                        if (variant == Enumerators.CardVariant.Standard)
                            continue;

                        CardKey variantCardKey = new CardKey(cardKey.MouldId, variant);
                        if (!gamechainCardKeys.Contains(variantCardKey))
                        {
                            _comparisonResult.CardVariantsMissing.Add(variantCardKey);
                        }
                    }
                }

                _comparisonResult.CardVariantsMissing =
                    _comparisonResult.CardVariantsMissing
                        .Where(key => !IgnoredCardMouldIds.Contains(key.MouldId.Id))
                        .OrderBy(key => key, CardKey.Comparer)
                        .ToList();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError(e.Message);
                _ownerWindow.ShowNotification(new GUIContent("Error: " + e.Message));
            }
        }

        private List<CardKey> ParseCardFaucetCardDetails(string json)
        {
            List<CardKey> cardKeys = new List<CardKey>();
            Dictionary<string, CardFaucetCard> cards =
                JsonConvert.DeserializeObject<Dictionary<string, CardFaucetCard>>(json, _jsonSerializerSettings);
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
            public List<CardKey> CardVariantsMissing;
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
    }
}
