namespace Loom.ZombieBattleground.Common
{
    public class Enumerators
    {
        public enum AbilityActivityType
        {
            PASSIVE,
            ACTIVE
        }

        public enum AbilityCallType
        {
            TURN,
            ENTRY,
            END,
            ATTACK,
            DEATH,
            PERMANENT,
            GOT_DAMAGE,
            AT_DEFENCE,
            IN_HAND
        }

        public enum AbilityEffectType
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
            SWING_LIGHTNING
        }

        public enum AbilityTargetType
        {
            NONE,
            PLAYER,
            PLAYER_CARD,
            PLAYER_ALL_CARDS,
            OPPONENT,
            OPPONENT_CARD,
            OPPONENT_ALL_CARDS,
            ALL_CARDS,
            ALL
        }

        public enum AbilityType
        {
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
            ADJACENT_UNITS_GET_GUARD
        }

        public enum ActionType
        {
            None,

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
            Card,
            Character
        }

        public enum AiType
        {
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
            HERO_SELECTION,
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

        public enum AttackInfoType
        {
            ANY,
            ONLY_DIFFERENT
        }

        public enum BuffType
        {
            GUARD,
            DEFENCE,
            HEAVY,
            WEAPON,
            RUSH,
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
            HEROES_DATA,
            COLLECTION_DATA,
            DECKS_DATA,
            DECKS_OPPONENT_DATA,
            USER_LOCAL_DATA,
            CREDITS_DATA,
            BUFFS_TOOLTIP_DATA
        }

        public enum CardKind
        {
            CREATURE,
            SPELL
        }

        public enum CardPackType
        {
            DEFAULT
        }

        public enum CardRank
        {
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
            WALKER,
            FERAL,
            HEAVY,
            NONE
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

        public enum OverlordSkill
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

        public enum SetType
        {
            FIRE,
            WATER,
            EARTH,
            AIR,
            LIFE,
            TOXIC,
            ITEM,
            OTHERS,
            NONE
        }

        public enum SkillTargetType
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
            WALKER_ATTACK_1,
            WALKER_ATTACK_2,
            HERO_DEATH,
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
            GOO_OVERFLOW_FADE_OUT
        }

        public enum StatType
        {
            HEALTH,
            DAMAGE,
            NONE
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

        public enum TutorialJanePoses
        {
            NORMAL,
            THINKING,
            POINTING,
            THUMBS_UP,
            KISS
        }

        public enum TutorialReportAction
        {
            NONE,
            END_TURN,
            MOVE_CARD,
            ATTACK_CARD_CARD,
            ATTACK_CARD_HERO,
            USE_ABILITY,
            HERO_DEATH,
            START_TURN,
            END_OF_RANK_UPGRADE
        }

        public enum UnitStatusType
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
            AddCardToHand
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
            Impact,
            Moving
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
            DontAttack
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
    }
}
