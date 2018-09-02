using UnityEngine;

namespace LoomNetwork.CZB.Common
{
    public class Constants
    {
        internal const string KCurrentVersionBase = "0.1.2";

        internal const string KCurrentVersionDevelopmentStage = "Alpha";

        internal const string KVersionFileResolution = ".ver";

        internal const string KLocalUserDataFilePath = "0412DD0.dat";

        internal const string KLocalCardsLibraryDataFilePath = "card_library_data.json";

        internal const string KLocalCollectionDataFilePath = "collection_data.json";

        internal const string KLocalHeroesDataFilePath = "heroes_data.json";

        internal const string KLocalDecksDataFilePath = "decks_data.json";

        internal const string KLocalOpponentDecksDataFilePath = "opponent_decks_data.json";

        internal const string KLocalOpponentActionsLibraryDataFilePath = "action_library_data.json";

        internal const string KLocalCreditsDataFilePath = "credits_data.json";

        internal const string KLocalBuffsTooltipDataFilePath = "buffs_tooltip_data.json";

        internal const string KPrivateEncryptionKeyForApp = "sagatdsgsg7687sdg587a8gs89";

        internal const string KTagPlayerOwned = "PlayerOwned";

        internal const string KTagOpponentOwned = "OpponentOwned";

        internal const string KPlayerBoard = "PlayerBoard";

        internal const string KOpponentBoard = "OpponentBoard";

        internal const string KLayerHandCards = "HandCards";

        internal const string KLayerBoardCards = "BoardCards";

        internal const string KLayerDefault = "Default";

        internal const string KLayerForeground = "Foreground";

        internal const string KLayerGameUI1 = "GameUI1";

        internal const string KLayerGameUI2 = "GameUI2";

        internal const string KLayerGameUI3 = "GameUI3";

        internal const string KZoneHand = "Hand";

        internal const string KZoneBoard = "Board";

        internal const string KZoneDeck = "Deck";

        internal const string KZoneGraveyard = "Graveyard";

        internal const string KTagLife = "Life";

        internal const string KTagDamage = "Damage";

        internal const string KTagHp = "HP";

        internal const string KTagMana = "Goo";

        internal const string KStatDamage = "DMG";

        internal const string KStatHp = "HP";

        internal const string KContentFolderName = "SpreadsheetsData/";

        internal const string KSpreadsheetFileFormat = ".csv";

        internal const string KCardSoundPlay = "P";

        internal const string KCardSoundAttack = "A";

        internal const string KCardSoundDeath = "D";

        internal const float KPointerOnClickDelay = 1.5f;

        internal const float KPointerMinDragDelta = 3f;

        internal const float KPointerMinDragDeltaMobile = 35f;

        internal const float KLoadingTimeBetweenGameplayAndAppInit = 2f;

        internal const int KTutorialPlayerHeroId = 4;

        internal static bool DataEncryptionEnabled =
#if !UNITY_EDITOR
            true;
#else
            false;
#endif

        internal static uint HeroesAmount = 9;

        internal static uint DeckMaxSize = 30;

        internal static uint CardItemMaxCopies = 2;

        internal static uint CardMinionMaxCopies = 4;

        internal static uint CardOfficerMaxCopies = 2;

        internal static uint CardCommanderMaxCopies = 2;

        internal static uint CardGeneralMaxCopies = 1;

        internal static uint CardsInPack = 5;

        internal static uint MaxBoardUnits = 6;

        internal static int DefaultTurnDuration = 60;

        internal static int MaxDeckSize = 30;

        internal static int MinDeckSize = 30;

        internal static int MaxDecksAtAll = 6;

        internal static int MaxCardsInHand = 10;

        internal static int DefaultPlayerHp = 20;

        internal static int DefaultPlayerGoo = 0;

        internal static int MaximumPlayerGoo = 10;

        internal static int FirstGameTurnIndex = 1;

        internal static int DefaultCardsInHandAtStartGame = 3;

        internal static float DelayBetweenGameplayActions = 0.01f;

        internal static int DelayBetweenAiActions = 1100;

        internal static Vector3 VfxOffset = Vector3.zero;

        // SOUNDS VOLUME'S
        internal static float ZombiesSoundVolume = 0.05f;

        internal static float TutorialSoundVolume = 1f;

        internal static float ArrivalSoundVolume = 0.1f;

        internal static float CreatureAttackSoundVolume = 0.05f;

        internal static float EndTurnClickSoundVolume = 0.1f;

        internal static float HeroDeathSoundVolume = 0.1f;

        internal static float CardsMoveSoundVolume = 0.025f;

        internal static float BackgroundSoundVolume = 0.05f;

        internal static float SfxSoundVolume = 0.15f;

        internal static float OverlordAbilitySoundVolume = 0.1f;

        internal static float SpellAbilitySoundVolume = 0.1f;

        internal static float BattlegroundEffectsSoundVolume = 0.25f;

        internal static float ZombieDeathVoDelayBeforeFadeout = 3f;

        internal static float DelayToPlayDeathSoundOfCreature = 2f;

        internal static float CardDistributionTime = 5f;
    }
}
