using UnityEngine;

namespace Loom.ZombieBattleground.Common
{
    public static class Constants
    {
        internal const string CurrentVersionBase = "0.1.23";

        internal const string VersionFileResolution = ".ver";

        internal const string LocalUserDataFileName = "0412DD0.dat";

        internal const string LocalCardsLibraryDataFileName = "card_library.json";

        internal const string LocalCreditsDataFileName = "credits_data.json";

        internal const string LocalBuffsTooltipDataFileName = "buffs_tooltip_data.json";

        internal const string LocalConfigDataFileName = "config_data.json";

        internal const string LocalCollectionDataFileName = "collection_data.json";

        // ReSharper disable once UnusedMember.Global
        internal const string PrivateEncryptionKeyForApp = "sagatdsgsg7687sdg587a8gs89";

        internal const string PlayerBoard = "PlayerBoard";

        internal const string OpponentBoard = "OpponentBoard";

        internal const string BattlegroundTouchZone = "BattlegroundTouchZona";

        internal const string PlayerManaBar = "PlayerManaBar";

        internal const string ContentFolderName = "SpreadsheetsData/";

        internal const string CardSoundPlay = "P";

        internal const string CardSoundAttack = "A";

        internal const string CardSoundDeath = "D";

        internal const float PointerOnClickDelay = 1.5f;

        internal const float PointerMinDragDelta = 3f;

        internal const float PointerMinDragDeltaMobile = 35f;

        internal const float LoadingTimeBetweenGameplayAndAppInit =
#if UNITY_EDITOR || DEVELOPMENT || DEVELOPMENT_BUILD
            0f;
#else
            2f;
#endif

        internal const int TutorialPlayerOverlordId = 4;

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

        internal const int TutorialDefaultOverlordDefense = 50;

        internal const int DefaultPlayerGoo = 0;

        internal const int MaximumPlayerGoo = 10;

        internal const int FirstGameTurnIndex = 1;

        internal const int DefaultCardsInHandAtStartGame = 3;

        internal const float DelayBetweenGameplayActions = 0.1f;

        internal const float MaxTimeForHovering = 3f;

        internal const int DelayBetweenAiActions = 1100;

        internal const float ZombieDeathVoDelayBeforeFadeout = 3f;

        internal const float DelayBetweenYourTurnPopup = 2.1f;

        internal static Vector3 VfxOffset = Vector3.zero;

        // SOUNDS VOLUME'S
        internal const float TutorialSoundVolume = 1f;

        internal const float ArrivalSoundVolume = 0.1f;

        internal const float EndTurnClickSoundVolume = 0.1f;

        internal const float OverlordDeathSoundVolume = 0.1f;

        internal const float CardsMoveSoundVolume = 0.05f;

        internal const float BackgroundSoundVolume = 0.1f;

        internal const float SfxSoundVolume = 0.15f;

        internal const float OverlordAbilitySoundVolume = 0.15f;

        internal const float SpellAbilitySoundVolume = 0.1f;

        internal const float BattlegroundEffectsSoundVolume = 0.20f;

        // HACK: must be const
        internal static float ZombiesSoundVolume = 0.05f;
        internal static float CreatureAttackSoundVolume = 0.05f;

        internal const string AssetBundleMain = "main";

        internal const string AssetBundleLoadingScreen = "loadingscreen";

        internal const string Space = " ";

        internal const string Empty = "";

        internal const string OverlordRegularNeckR = "OverlordRegularNeckR";

        internal const string OverlordRegularNeckL = "OverlordRegularNeckL";

        internal const string Player = "Player";

        internal const string Opponent = "Opponent";

        internal const float TurnTime = 70;

        internal const float ShortTurnTime = 15;

        internal const int MaxDecksCount = 10;

        public const float PvPCheckPlayerAvailableMaxTime = 30f;

        internal const float TimeForStartEndTurnAnimation = 15;

        internal const float DefaultPositonOfUnitWhenSpawn = 1.9f;

        internal const float OverlordTalkingPopupDuration = 2f;

        internal const float OverlordTalkingPopupMinimumShowTime = 2f;

        internal const float DescriptionTooltipMinimumShowTime = 2f;

        internal const float HandPointerSpeed = 4f;

        internal const float DurationHoveringHandCard = 0.15f;

        internal const float DurationUnitAttacking = 0.65f;

        internal const float DurationEndUnitAttacking = 0.1f;

        internal const int BackendCallTimeout = 10000;

        internal const int PlasmachainCallTimeout = 20000;

        internal const float QueueActionTimeout = 60f;    

        internal static Vector3 DefaultPositionOfPlayerBoardCard = new Vector3(6.5f, -2.5f, 0);
        internal static Vector3 DefaultPositionOfOpponentBoardCard = new Vector3(6.5f, 3.5f, 0);

        internal static Vector3 LeftPlayerOverlordPositionForChat = new Vector3(-3.95f, -4.65f, 0);
        internal static Vector3 RightPlayerOverlordPositionForChat = new Vector3(3.95f, -4.65f, 0);
        internal static Vector3 LeftOpponentOverlordPositionForChat = new Vector3(-3.95f, 5.7f, 0);
        internal static Vector3 RightOpponentOverlordPositionForChat = new Vector3(3.95f, 5.7f, 0);

        internal static Vector3 DefaultScaleForZoomedCardInHand = new Vector3(0.375f, 0.375f, 0.375f);

        public const bool MulliganEnabled = true;

        public static readonly bool AlwaysGuestLogin = false;

        public const string VaultEmptyErrorCode = "NotFound";

        public static readonly bool DevModeEnabled = false;

        public const bool UsingCardTooltips = false;

        public static readonly bool RankSystemEnabled = false;

        public const bool GameStateValidationEnabled =
#if UNITY_EDITOR
            true;
#else
            false;
#endif

        public const string MatchEmailPattern =
            @"^(([^<>()\[\]\.,;:\s@""]+(\.[^<>()\[\]\.,;:\s@""]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";

        internal const string FacebookAppId =
#if USE_STAGING_BACKEND
            "1985151694912169";
#else
            "1985151694912169";
#endif

        internal static readonly bool EnableShopPage =
#if UNITY_EDITOR || DEVELOPMENT || DEVELOPMENT_BUILD
            true;
#else
            false;
#endif

        internal const int LastTutorialId = 8;

        internal const string MarketPlaceLink = "https://loom.games/en/purchase";
        
        internal const string HelpLink = "https://loom.games/en/how-to-play";

        internal const string SupportLink = "https://loom.freshdesk.com/support/home";

        internal const string PathToCardsIllustrations = "Images/Cards/Illustrations/";

        internal const string ZbVersionLink = "/zbversion?environment=" + EnvironmentPointText + "&version=" + CurrentVersionBase;

        internal const string EnvironmentPointText = "%environment_point%";
              
        internal const int MinimumMemorySize = 2048;
        
        internal const float MinimumMemoryThresholdPercentage = 0.85f;
    }
}
