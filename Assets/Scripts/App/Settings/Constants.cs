using UnityEngine;

namespace Loom.ZombieBattleground.Common
{
    public static class Constants
    {
        internal const string CurrentVersionBase = "0.1.4";

        internal const string CurrentVersionDevelopmentStage = "Alpha";

        internal const string VersionFileResolution = ".ver";

        internal const string LocalUserDataFilePath = "0412DD0.dat";

        internal const string LocalCardsLibraryDataFilePath = "card_library_data.json";

        internal const string LocalCollectionDataFilePath = "collection_data.json";

        internal const string LocalHeroesDataFilePath = "heroes_data.json";

        internal const string LocalDecksDataFilePath = "decks_data.json";

        internal const string LocalOpponentDecksDataFilePath = "opponent_decks_data.json";

        internal const string LocalCreditsDataFilePath = "credits_data.json";

        internal const string LocalBuffsTooltipDataFilePath = "buffs_tooltip_data.json";

        internal const string LocalTutorialDataFilePath = "tutorial_data.json";

        // ReSharper disable once UnusedMember.Global
        internal const string PrivateEncryptionKeyForApp = "sagatdsgsg7687sdg587a8gs89";

        internal const string PlayerBoard = "PlayerBoard";

        internal const string OpponentBoard = "OpponentBoard";

        internal const string ContentFolderName = "SpreadsheetsData/";

        internal const string CardSoundPlay = "P";

        internal const string CardSoundAttack = "A";

        internal const string CardSoundDeath = "D";

        internal const float PointerOnClickDelay = 1.5f;

        internal const float PointerMinDragDelta = 3f;

        internal const float PointerMinDragDeltaMobile = 35f;

        internal const float LoadingTimeBetweenGameplayAndAppInit = 2f;

        internal const int TutorialPlayerHeroId = 4;

        internal const uint DeckMaxSize = 30;

        internal const uint CardItemMaxCopies = 2;

        internal const uint CardMinionMaxCopies = 4;

        internal const uint CardOfficerMaxCopies = 2;

        internal const uint CardCommanderMaxCopies = 2;

        internal const uint CardGeneralMaxCopies = 1;

        internal const uint CardsInPack = 5;

        internal const uint MaxBoardUnits = 6;

        internal const int MaxDeckSize = 30;

        internal const int MinDeckSize = 30;

        internal const int MaxCardsInHand = 10;

        internal const int DefaultPlayerHp = 20;

        internal const int DefaultPlayerGoo = 0;

        internal const int MaximumPlayerGoo = 10;

        internal const int FirstGameTurnIndex = 1;

        internal const int DefaultCardsInHandAtStartGame = 3;

        internal const float DelayBetweenGameplayActions = 0.01f;

        internal const int DelayBetweenAiActions = 1100;

        internal const float TutorialSoundVolume = 1f;

        internal const float ArrivalSoundVolume = 0.1f;

        internal const float EndTurnClickSoundVolume = 0.1f;

        internal const float HeroDeathSoundVolume = 0.1f;

        internal const float CardsMoveSoundVolume = 0.025f;

        internal const float BackgroundSoundVolume = 0.05f;

        internal const float SfxSoundVolume = 0.15f;

        internal const float OverlordAbilitySoundVolume = 0.1f;

        internal const float SpellAbilitySoundVolume = 0.1f;

        internal const float BattlegroundEffectsSoundVolume = 0.25f;

        internal const float ZombieDeathVoDelayBeforeFadeout = 3f;

        internal static Vector3 VfxOffset = Vector3.zero;

        // SOUNDS VOLUME'S

        // HACK: must be const
        internal static float ZombiesSoundVolume = 0.05f;
        internal static float CreatureAttackSoundVolume = 0.05f;

        internal const string AssetBundleMain = "main";

        internal const string AssetBundleLoadingScreen = "loadingscreen";
    }
}
