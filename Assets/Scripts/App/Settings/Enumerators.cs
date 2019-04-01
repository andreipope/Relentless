using System.Runtime.Serialization;

namespace Loom.ZombieBattleground.Common
{
    public class Enumerators
    {
        public enum AbilityTrigger
        {
            Undefined,

            Entry,
            EntryWithSelection,
            Turn,
            End,
            Death,
            Attack,
            KillUnit,
            AtDefense,
            StatChanged,
            InHand,
            Static
        }

        public enum Target
        {
            Undefined,

            ItSelf,
            Player,
            PlayerCard,
            Opponent,
            OpponentCard,
            All
        }

        public enum TargetFilter
        {
            Undefined,

            Target,
            TargetAdjustments
        }

        public enum AbilityType
        {
            Undefined,

            Blitz,
            Damage,
            Freeze,
            ChangeStat,
            Draw,
            GainGoo,
            ChangeCost,
            Heal,
            ChangeType,
            TakeControl,
            Swing,
            Rage,
            Shield,
            Reanimate,
            CardReturn,
            PriorityAttack,
            OverflowGoo,
            Distract,
            DisableGoo,
            Devour,
            Destroy,
            Summon,
            Flash,
            AddCard,
            PlaceCopies,
            ShuffleCard,
            ReplaceOnStrongerOnes,
            AdditionalDamage
        }

        public enum AbilityParameter
        {
            Undefined,

            Defense,
            Attack,
            Delay,
            Damage,

            Stat,
            TargetStat,

            Faction,
            TargetFaction,

            Turns
        }

        public enum ActionType
        {
            Undefined,

            PlayCardFromHand,
            PlayCardFromHandOnCard,
            PlayCardFromHandOnMultipleCards,
            PlayCardFromHandOnOverlord,
            PlayCardFromHandOncardsWithOverlord,
            UseOverlordPower,
            UseOverlordPowerOnCard,
            UseOverlordPowerOnMultilpleCards,
            UseOverlordPowerOnOverlord,
            UseOverlordPowerOnCardsWithOverlord,
            CardAttackCard,
            CardAttackOverlord,
            CardAffectingCard,
            CardAffectingMultipleCards,
            CardAffectingOverlord,
            CardAffectingCardsWithOverlord
        }

        public enum AffectObjectType
        {
            None,
            Player,
            Character,
            Card
        }

        public enum AIType
        {
            UNDEFINED,
            BLITZ_AI,
            DEFENSE_AI,
            MIXED_AI,
            MIXED_BLITZ_AI,
            TIME_BLITZ_AI,
            MIXED_DEFENSE_AI
        }

        public enum AppState
        {
            NONE,
            APP_INIT,
            LOGIN,
            MAIN_MENU,
            OVERLORD_SELECTION,
            HordeSelection,
            ARMY,
            SHOP,
            GAMEPLAY,
            DECK_EDITING,
            PACK_OPENER,
            CREDITS,
            PlaySelection,
            PvPSelection,
            CustomGameModeList,
            CustomGameModeCustomUi
        }

        public enum AttackRestriction
        { 
            ANY,
            ONLY_DIFFERENT
        }

        public enum GameMechanicDescription
        {
            [EnumMember(Value = "UNDEFINED")]
            Undefined,

            [EnumMember(Value = "ATTACK")]
            Attack,

            [EnumMember(Value = "DEATH")]
            Death,

            [EnumMember(Value = "DELAYED")]
            DelayedX,

            [EnumMember(Value = "DESTROY")]
            Destroy,

            [EnumMember(Value = "DEVOUR")]
            Devour,

            [EnumMember(Value = "DISTRACT")]
            Distract,

            [EnumMember(Value = "END")]
            End,

            [EnumMember(Value = "ENTRY")]
            Entry,

            [EnumMember(Value = "FERAL")]
            Feral,

            [EnumMember(Value = "FLASH")]
            Flash,

            [EnumMember(Value = "FREEZE")]
            Freeze,

            [EnumMember(Value = "GUARD")]
            Guard,

            [EnumMember(Value = "HEAVY")]
            Heavy,

            [EnumMember(Value = "OVERFLOW")]
            OverflowX,

            [EnumMember(Value = "RAGE")]
            RageX,

            [EnumMember(Value = "REANIMATE")]
            Reanimate,

            [EnumMember(Value = "SHATTER")]
            Shatter,

            [EnumMember(Value = "SWING")]
            SwingX,

            [EnumMember(Value = "TURN")]
            Turn,

            [EnumMember(Value = "GOT_DAMAGE")]
            GotDamage,

            [EnumMember(Value = "AT_DEFENSE")]
            AtDefense,

            [EnumMember(Value = "IN_HAND")]
            InHand,

            [EnumMember(Value = "KILL_UNIT")]
            KillUnit,

            [EnumMember(Value = "PERMANENT")]
            Permanent,

            [EnumMember(Value = "BLITZ")]
            Blitz,

            [EnumMember(Value = "RESTORE")]
            Restore,

            [EnumMember(Value = "CHAINSAW")]
            Chainsaw,

            [EnumMember(Value = "SUPER_SERUM")]
            SuperSerum,
        }

        public enum BuffType
        {
            NONE,
            GUARD,
            DEFENCE,
            HEAVY,
            WEAPON,
            BLITZ,
            ATTACK,
            FREEZE,
            DAMAGE,
            HEAL_ALLY,
            DESTROY,
            REANIMATE
        }

        public enum CacheDataType
        {
            CARDS_LIBRARY_DATA,
            OVERLORDS_DATA,
            COLLECTION_DATA,
            DECKS_DATA,
            DECKS_OPPONENT_DATA,
            USER_LOCAL_DATA,
            CREDITS_DATA,
            BUFFS_TOOLTIP_DATA
        }

        public enum CardKind
        {
            UNDEFINED,
            CREATURE,
            ITEM
        }

        public enum CardPackType
        {
            DEFAULT
        }

        public enum CardRank
        {
            UNDEFINED,
            MINION,
            OFFICER,
            COMMANDER,
            GENERAL
        }

        public enum CardSoundType
        {
            NONE,
            ATTACK,
            DEATH,
            PLAY
        }

        public enum CardType
        {
            UNDEFINED,
            WALKER,
            FERAL,
            HEAVY,
        }

        public enum EndGameType
        {
            WIN,
            LOSE,
            CANCEL
        }

        public enum FadeState
        {
            DEFAULT,
            FADED
        }

        public enum InputType
        {
            KEYBOARD = 0,
            MOUSE,
            TOUCH
        }

        public enum Language
        {
            NONE,
            DE,
            EN,
            RU
        }

        public enum MatchType
        {
            LOCAL,
            PVP,
            PVE
        }

        public enum Skill
        {
            NONE,

            // AIR
            PUSH,
            DRAW,
            WIND_SHIELD,
            LEVITATE,
            RETREAT,

            // EARTH
            HARDEN,
            STONE_SKIN,
            FORTIFY,
            PHALANX,
            FORTRESS,

            // FIRE
            FIRE_BOLT,
            RABIES,
            FIREBALL,
            MASS_RABIES,
            METEOR_SHOWER,

            // LIFE
            HEALING_TOUCH,
            MEND,
            RESSURECT,
            ENHANCE,
            REANIMATE,

            // TOXIC
            POISON_DART,
            TOXIC_POWER,
            BREAKOUT,
            INFECT,
            EPIDEMIC,

            // WATER
            FREEZE,
            ICE_BOLT,
            ICE_WALL,
            SHATTER,
            BLIZZARD
        }

        public enum Faction
        {
            Undefined,
            FIRE,
            WATER,
            EARTH,
            AIR,
            LIFE,
            TOXIC,
            ITEM,
        }

        public enum SkillTarget
        {
            NONE,
            PLAYER,
            PLAYER_CARD,
            PLAYER_ALL_CARDS,
            OPPONENT,
            OPPONENT_CARD,
            OPPONENT_ALL_CARDS,
            ALL_CARDS
        }

        public enum SkillType
        {
            PRIMARY,
            SECONDARY
        }

        public enum SoundType
        {
            CLICK,
            // OTHER,
            BACKGROUND,
            BATTLEGROUND,
            BATTLEGROUND_TOUCH_EFFECT,
            TUTORIAL,
            CARDS,
            END_TURN,
            OVERLORD_ABILITIES,
            SPELLS,
            WALKER_ARRIVAL,
            FERAL_ARRIVAL,
            HEAVY_ARRIVAL,
            FERAL_ATTACK,
            HEAVY_ATTACK_1,
            HEAVY_ATTACK_2,
            WALKER_ATTACK,
            HERO_DEATH,
            HERO_DEATH_AIR,
            HERO_DEATH_EARTH,
            HERO_DEATH_FIRE,
            HERO_DEATH_TOXIC,
            HERO_DEATH_WATER,
            HERO_DEATH_LIFE,
            LOGO_APPEAR,
            CARD_BATTLEGROUND_TO_TRASH,
            CARD_DECK_TO_HAND_MULTIPLE,
            CARD_DECK_TO_HAND_SINGLE,
            CARD_FLY_HAND,
            CARD_FLY_HAND_TO_BATTLEGROUND,
            CHANGE_SCREEN,
            DECKEDITING_ADD_CARD,
            DECKEDITING_REMOVE_CARD,
            LOST_POPUP,
            WON_POPUP,
            WON_REWARD_POPUP,
            YOURTURN_POPUP,
            SHUTTERS_CLOSING,
            SHUTTERS_OPEN,
            GOO_OVERFLOW_FADE_IN,
            GOO_OVERFLOW_FADE_LOOP,
            GOO_OVERFLOW_FADE_OUT,
            GOO_TUBE_LOOP,
            GOO_BOTTLE_FILLING,
            PREPARING_FOR_BATTLE,
            PREPARING_FOR_BATTLE_LOOP,
            DISTRACT_LOOP,
            RAGE_LOOP,
            UNIQUE_ARRIVALS,
            ZOMBIE_DEATH_ANIMATIONS,
            OPEN_PACK,
            CARD_REVEAL_MINION,
            CARD_REVEAL_OFFICER,
            CARD_REVEAL_GENERAL,
            CARD_REVEAL_COMMANDER
        }

        public enum Stat
        {
            UNDEFINED,
            DEFENSE,
            DAMAGE
        }

        public enum StunType
        {
            NONE,
            FREEZE,
            DISABLE
        }

        public enum TooltipObjectType
        {
            RANK,
            ABILITY,
            UNIT_TYPE,
            BUFF
        }

        public enum UnitSpecialStatus
        {
            NONE,
            FROZEN
        }

        public enum ActionEffectType
        {
            None,

            AttackBuff,
            AttackDebuff,
            ShieldBuff,
            ShieldDebuff,

            Feral,
            Heavy,

            Damage,
            LifeGain,

            Blitz,
            DeathMark,
            Guard,
            Overflow,
            Freeze,

            Push,
            Reanimate,
            LowGooCost,
            ReturnToHand,

            SpawnOnBoard,
            AddCardToHand,
            Distract,
            PlayRandomCardOnBoardFromDeck,
            PlayFromHand,
            Swing,
            Devour
        }
        public enum ScreenMode
        {
            FullScreen,
            Window,
            BorderlessWindow
        }

        public enum ExperienceActionType
        {
            KillOverlord,
            KillMinion,
            PlayCard,
            ActivateRankAbility,
            UseOverlordAbility
        }

        public enum VisualEffectType
        {
            Undefined,
            Impact,
            Moving,
            Impact_Heavy,
            Impact_Feral
        }

        public enum ShutterState
        {
            Open,
            Close
        }

        public enum AiBrainType
        {
            DoNothing,
            Normal,
            DontAttack,
            Tutorial
        }

        public enum StartingTurn
        {
            UnDecided,
            Player,
            Enemy
        }

        public enum PlayerActionType
        {
            PlayCardOnBoard,
            AttackOnUnit,
            AttackOnOverlord,
            PlayOverlordSkill
        }

        public enum AbilitySubTrigger
        {
            None,
            OnlyThisUnitInPlay,
            AllOtherAllyUnitsInPlay,
            AllAllyUnitsInPlay,
            RandomUnit,
            AllEnemyUnitsInPlay,
            AllAllyUnitsByFactionInPlay,
            ForEachFactionOfUnitInHand,
            IfHasUnitsWithFactionInPlay,
            AllyUnitsByFactionThatCost,
            YourOverlord
        }

        public enum UniqueAnimation
        {
            None,
            ShammannArrival,
            ZVirusArrival,
            ZeuzArrival,
            CerberusArrival,
            TzunamyArrival,
            ChernoBillArrival
        }

        public enum CardNameOfAbility
        {
            None,
            Bulldozer,
            Lawnmover
        }

        public enum AbilityEffectInfoPositionType
        {
            Target,
            Overlord
        }

        public enum ShakeType
        {
            Short,
            Medium,
            Long
        }

        public enum MatchPlayer
        {
            CurrentPlayer,
            OpponentPlayer
        }

        public enum QueueActionType
        {
            CardPlay,
            RankBuff,
            AbilityUsage,
            UnitDeath,
            WholeBoardUpdate,
            PlayerBoardUpdate,
            OpponentBoardUpdate,
            OverlordSkillUsage,
            AbilityUsageBlocker,
            StopTurn,
            EndMatch,
            UnitCombat,
            LeaveMatch
        }

        public enum TooltipAlign
        {
            Undefined,

            TopLeft,
            TopMiddle,
            TopRight,
            CenterLeft,
            CenterMiddle,
            CenterRight,
            BottomLeft,
            BottomMiddle,
            BottomRight
        }

        public enum TutorialObjectOwner
        {
            Undefined,

            EnemyOverlord,
            EnemyBattleframe,
            EnemyCardInHand,

            PlayerOverlord,
            PlayerBattleframe,
            PlayerCardInHand,
            PlayerGooBottles,
            PlayerOverlordAbility,

            UI,
            IncorrectButton,

            Battleframe,
            HandCard
        }

        public enum TutorialActivityAction
        {
            Undefined,

            PlayerOverlordTriedToAttackTargetWhenItsLimited,
            EnemyOverlordDied,
            EnemyOverlordCardPlayedStarted,
            EnemyOverlordCardPlayed,
            EnemyOverlordSelected,

            PlayerOverlordCardDrawed,
            PlayerOverlordTriedToPlayUnsequentionalCard,
            PlayerOverlordTriedToUseUnsequentionalBattleframe,
            PlayerOverlordTriedToUseWrongBattleframe,
            PlayerOverlordTriedToPlayCardWhenItsLimited,
            PlayerOverlorDied,
            PlayerOverlordAbilityUsed,
            PlayerOverlordCardPlayed,
            PlayerOverlordSelected,
            PlayerManaBarSelected,
            PlayerCardInHandSelected,
            DeathAbilityCompleted,

            PlayerCreatedNewCardAndMovedToHand,

            EndMatchPopupAppear,

            TapOnDisabledButtonWhenItsLimited,
            TapOnEndTurnButtonWhenItsLimited,

            EndTurn,
            StartTurn,
            RanksUpdated,

            BattleframeAttacked,
            BattleframeSelected,
            BattleframeDeselected,

            PlayerBattleframeDied,
            EnemyBattleframeDied,

            TapOnScreen,
            AvatarTooltipClosed,
            DescriptionTooltipClosed,

            OverlordSayPopupHided,

            YouWonPopupOpened,
            YouWonPopupClosed,

            YouLosePopupOpened,
            YouLosePopupClosed,

            TutorialProgressInfoPopupClosed,

            ScreenChanged,
            PopupClosed,

            CardPackOpened,
            CardPackCollected,

            CardDragged,
            CardRemoved,
            CardAdded,

            HordeSaved,
            HordeFilled,

            IncorrectButtonTapped,
            TriedToPressPlayButton,

            SceneChanged,

            BattleStarted,

            CardWithAbilityPlayed,

            EndCardFlipPlayerOrderPopup,

            HordeTabChanged,

            HordeSaveButtonPressed
        }

        public enum TutorialActivityActionHandler
        {
            Undefined,

            OverlordSayTooltip,
            DrawDescriptionTooltips,
        }

        public enum TutorialHandPointerType
        {
            Single,
            Animated
        }

        public enum TutorialAvatarPose
        {
            NORMAL,
            THINKING,
            POINTING,
            THUMBS_UP,
            KISS
        }

        public enum TutorialHandState
        {
            Drag,
            Pointing,
            Pressed
        }

        public enum TutorialObjectLayer
        {
            Default,
            AbovePages,
            AbovePopups,
            AboveUI
        }
        
        public enum MarketplaceCardPackType
        {
            Booster = 0,
            Super = 1,
            Air = 2,
            Earth = 3,
            Fire = 4,
            Life = 5,
            Toxic = 6,
            Water = 7,
            Small = 8,
            Minion = 9            
        }

        public enum TutorialStepType
        {
            MenuStep,
            GameplayStep
        }
    }
}
