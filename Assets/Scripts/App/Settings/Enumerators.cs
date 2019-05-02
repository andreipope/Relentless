using System;
using System.Runtime.Serialization;

namespace Loom.ZombieBattleground.Common
{
    public class Enumerators
    {
        public enum AbilityActivity
        {
            UNDEFINED,
            PASSIVE,
            ACTIVE
        }

        public enum AbilityTrigger
        {
            UNDEFINED,
            TURN,
            ENTRY,
            END,
            ATTACK,
            DEATH,
            PERMANENT,
            GOT_DAMAGE,
            AT_DEFENCE,
            IN_HAND,
            KILL_UNIT,
            RAGE,
            AURA
        }

        public enum AbilityEffect
        {
            NONE,
            MASSIVE_WATER_WAVE,
            MASSIVE_FIRE,
            MASSIVE_LIGHTNING,
            MASSIVE_TOXIC_ALL,
            TARGET_ROCK,
            TARGET_FIRE,
            TARGET_LIFE,
            TARGET_TOXIC,
            TARGET_WATER,
            TARGET_ADJUSTMENTS_BOMB,
            STUN_FREEZES,
            STUN_OR_DAMAGE_FREEZES,
            TARGET_ADJUSTMENTS_AIR,
            HEAL_DIRECTLY,
            HEAL,
            SWING_LIGHTNING,
            CHANGE_STAT_FRESH_MEAT
        }

        public enum Target
        {
            NONE,
            PLAYER,
            PLAYER_CARD,
            PLAYER_ALL_CARDS,
            OPPONENT,
            OPPONENT_CARD,
            OPPONENT_ALL_CARDS,
            ALL_CARDS,
            ALL,
            ITSELF,

            BLITZ
        }

        public enum AbilityType
        {
            UNDEFINED,
            HEAL,
            MODIFICATOR_STATS,
            CHANGE_STAT,
            STUN,
            STUN_OR_DAMAGE_ADJUSTMENTS,
            SPURT,
            ADD_GOO_VIAL,
            ADD_GOO_CARRIER,
            DOT,
            SUMMON,
            SPELL_ATTACK,
            MASSIVE_DAMAGE,
            DAMAGE_TARGET_ADJUSTMENTS,
            DAMAGE_TARGET,
            CARD_RETURN,
            WEAPON,
            CHANGE_STAT_OF_CREATURES_BY_TYPE,
            ATTACK_NUMBER_OF_TIMES_PER_TURN,
            DRAW_CARD,
            DEVOUR_ZOMBIES_AND_COMBINE_STATS,
            DESTROY_UNIT_BY_TYPE,
            LOWER_COST_OF_CARD_IN_HAND,
            OVERFLOW_GOO,
            LOSE_GOO,
            DISABLE_NEXT_TURN_GOO,
            RAGE,
            FREEZE_UNITS,
            TAKE_DAMAGE_RANDOM_ENEMY,
            TAKE_CONTROL_ENEMY_UNIT,
            GUARD,
            DESTROY_FROZEN_UNIT,
            USE_ALL_GOO_TO_INCREASE_STATS,
            FIRST_UNIT_IN_PLAY,
            ALLY_UNITS_OF_TYPE_IN_PLAY_GET_STATS,
            DAMAGE_ENEMY_UNITS_AND_FREEZE_THEM,
            RETURN_UNITS_ON_BOARD_TO_OWNERS_DECKS,
            TAKE_UNIT_TYPE_TO_ADJACENT_ALLY_UNITS,
            ENEMY_THAT_ATTACKS_BECOME_FROZEN,
            TAKE_UNIT_TYPE_TO_ALLY_UNIT,
            REVIVE_DIED_UNITS_OF_TYPE_FROM_MATCH,
            CHANGE_STAT_UNTILL_END_OF_TURN,
            ATTACK_OVERLORD,
            ADJACENT_UNITS_GET_HEAVY,
            FREEZE_NUMBER_OF_RANDOM_ALLY,
            [Obsolete("Ability not used, and mould ids are used now", true)]
            ADD_CARD_BY_NAME_TO_HAND,
            DEAL_DAMAGE_TO_THIS_AND_ADJACENT_UNITS,
            SWING,
            TAKE_DEFENSE_IF_OVERLORD_HAS_LESS_DEFENSE_THAN,
            GAIN_NUMBER_OF_LIFE_FOR_EACH_DAMAGE_THIS_DEALS,
            ADDITIONAL_DAMAGE_TO_HEAVY_IN_ATTACK,
            UNIT_WEAPON,
            TAKE_DAMAGE_AT_END_OF_TURN_TO_THIS,
            DELAYED_LOSE_HEAVY_GAIN_ATTACK,
            DELAYED_GAIN_ATTACK,
            REANIMATE_UNIT,
            PRIORITY_ATTACK,
            DESTROY_TARGET_UNIT_AFTER_ATTACK,
            COSTS_LESS_IF_CARD_TYPE_IN_HAND,
            RETURN_UNITS_ON_BOARD_TO_OWNERS_HANDS,
            REPLACE_UNITS_WITH_TYPE_ON_STRONGER_ONES,
            RESTORE_DEF_RANDOMLY_SPLIT,
            ADJACENT_UNITS_GET_GUARD,
            SUMMON_UNIT_FROM_HAND,
            DAMAGE_AND_DISTRACT_TARGET,
            DRAW_CARD_IF_DAMAGED_ZOMBIE_IN_PLAY,
            TAKE_STAT_IF_OVERLORD_HAS_LESS_DEFENSE_THAN,
            DAMAGE_OVERLORD_ON_COUNT_ITEMS_PLAYED,
            SHUFFLE_THIS_CARD_TO_DECK,
            TAKE_DEFENSE_TO_OVERLORD_WITH_DEFENSE,
            PUT_RANDOM_UNIT_FROM_DECK_ON_BOARD,
            DISTRACT,
            DAMAGE_TARGET_FREEZE_IT_IF_SURVIVES,
            DESTROY_UNIT_BY_COST,
            DAMAGE_ENEMY_OR_RESTORE_DEFENSE_ALLY,
            TAKE_SWING_TO_UNITS,
            DELAYED_PLACE_COPIES_IN_PLAY_DESTROY_UNIT,
            ADJACENT_UNITS_GET_STAT,
            EXTRA_GOO_IF_UNIT_IN_PLAY,
            DESTROY_UNITS,
            DEAL_DAMAGE_TO_UNIT_AND_SWING,
            SET_ATTACK_AVAILABILITY,
            CHOOSABLE_ABILITIES,
            COSTS_LESS_IF_CARD_TYPE_IN_PLAY,
            GAIN_GOO,
            BLITZ,
            DRAW_CARD_BY_FACTION,
            DESTROY_TARGET_UNIT,
            AGILE,
            CHANGE_STAT_OF_CARDS_IN_HAND,
            GIVE_BUFFS_TO_UNIT,
            DISCARD_CARD_FROM_HAND,
            GET_GOO_THIS_TURN,
            COSTS_LESS,
            FILL_BOARD_BY_UNITS,
            DEAL_DAMAGE_TO_TARGET_THAT_ATTACK_THIS,
            CHANGE_COST,
            GAIN_STATS_OF_ADJACENT_UNITS,
            DISTRACT_AND_CHANGE_STAT,
            DAMAGE_AND_DISTRACT,
            PUT_UNITS_FROM_DISCARD_INTO_PLAY,
            PUT_UNITS_FRON_LIBRARY_INTO_PLAY,
            BLOCK_TAKE_DAMAGE,
            TAKE_UNIT_TYPE_TO_TARGET_UNIT,
            CHANGE_STAT_THIS_TURN
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

            [EnumMember(Value = "AURA")]
            Aura
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
            Devour,

            CardDiscard
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
            YourOverlord,
            PermanentChanges,
            HasChangesInParameters,
            LessDefThanInOpponent,
            OverlordDefenseEqualOrLess,
            OnlyThisCardInHand,
            AllCardsInHand,
            CardCostMoreThanCostOfThis,
            ForEachEnemyUnitInPlay,
            ForEachAllyUnitInPlay,
            IfHaveFewerUnitsInPlay,
            ToOpponentOverlord,
            UntilStartOfNextPlayerTurn,
            DuringCardInHand,
            ForEachUnitInPlay,
            HighestCost,
            EqualToUnitAttack,
            NumberOfUnspentGoo
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
            OverlordSkillUsage,
            AbilityUsageBlocker,
            AbilityTargetingBlocker,
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

        public enum ReasonForValueChange
        {
            Attack,
            AbilityBuff,
            AbilityDamage
        }
    }
}
