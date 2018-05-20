﻿namespace GrandDevs.CZB.Common
{
    public class Enumerators
    {
        public enum CardSound
        {
            PLAY,
            ATTACK,
            DEATH,
        }

        public enum AppState
        {
            NONE,
            APP_INIT,
            LOGIN,
			MAIN_MENU,
            HERO_SELECTION,
			DECK_SELECTION,
            COLLECTION,
            SHOP,
            GAMEPLAY,
            DECK_EDITING,
            PACK_OPENER,
            CREDITS
        }

        public enum ButtonState
        {
            ACTIVE,
            DEFAULT
        }

        public enum SoundType : int
        {
            CLICK,
            OTHER,
            BACKGROUND,
            TUTORIAL
        }

        public enum NotificationType
        {
            LOG,
            ERROR,
            WARNING,

            MESSAGE
        }

        public enum Language
        {
            NONE,

            DE,
            EN,
            RU
        }

        public enum ScreenOrientationMode
        {
            PORTRAIT,
            LANDSCAPE
        }

        public enum CacheDataType
        {
			CARDS_LIBRARY_DATA,
			HEROES_DATA,
			COLLECTION_DATA,
            DECKS_DATA,
            DECKS_OPPONENT_DATA,
            USER_LOCAL_DATA,
            OPPONENT_ACTIONS_LIBRARY_DATA,
            CREDITS_DATA
        }

        public enum NotificationButtonState
        {
            ACTIVE,
            INACTIVE
        }

        public enum MouseCode
        {
            LEFT_MOUSE_BUTTON = 0,
            RIGHT_MOUSE_BUTTON,
            WHEEL_MOUSE,
            OTHER
        }

        public enum InputType
        {
            KEYBOARD = 0,
            MOUSE,
            TOUCH
        }

        public enum FadeState
        {
            DEFAULT,
            FADED
        }

        public enum ElementType
        {
            FIRE,
            WATER,
            EARTH,
            AIR,
            LIFE,
            TOXIC,
            ITEMS
        }

        public enum SkillType
        {
            FREEZE,
            TOXIC_DAMAGE,
            FIRE_DAMAGE,
            HEAL_ANY,
            HEAL,
            CARD_RETURN
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

        public enum CardKind
        {
            CREATURE,
            SPELL,
        }

        public enum CardType
        {
            WALKER,
            FERAL,
            HEAVY
        }

        public enum CardRarity
        {
            COMMON,
            RARE,
            LEGENDARY,
            EPIC
        }

        public enum GameEndCondition
        {
            LIFE,
            TIME,
            TURN,
        }

        // abilities
        // TODO should be changed I guess
        public enum AbilityType
        {
            HEAL,
            MODIFICATOR_STATS,
            CHANGE_STAT,
			STUN,
			STUN_OR_DAMAGE_ADJUSTMENTS,
            SPURT,
            ADD_GOO_VIAL,
            DOT,
            SUMMON,
            SPELL_ATTACK,
            MASSIVE_DAMAGE,
			DAMAGE_TARGET_ADJUSTMENTS,
			DAMAGE_TARGET,
            CARD_RETURN,
            WEAPON,
        }

        public enum AbilityActivityType
        {
            PASSIVE,
            ACTIVE
        }

        public enum AbilityCallType
        {
            TURN_START,
            AT_START,
            AT_END,
            PERMANENT,
            AT_ATTACK,
            AT_DEATH,
        }

        public enum StatType
        {
            HEALTH,
            DAMAGE
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

        public enum AffectObjectType
        {
            NONE,
            PLAYER,
            CARD,
            CHARACTER
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

        public enum TutorialReportAction
        {
            END_TURN,
            MOVE_CARD,
            ATTACK_CARD_CARD,
            ATTACK_CARD_HERO,
            USE_ABILITY,
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
            TARGET_ADJUSTMENTS_BOMB,
            STUN_FREEZES,
            STUN_OR_DAMAGE_FREEZES,
            TARGET_ADJUSTMENTS_AIR,
            HEAL_DIRECTLY,
            HEAL
        }

        public enum SpreadsheetType
        {
            TUTORIAL
        }

        public enum ActionType
        {
            TEST,
            TEST2
        }

        public enum EffectActivateType
        {
            PLAY_SKILL_EFFECT
        }

        public enum AIType
        {
            BLITZ_AI,
            DEFENSE_AI,
            MIXED_AI,
            MIXED_BLITZ_AI,
            TIME_BLITZ_AI,
            MIXED_DEFENSE_AI,
        }

        public enum TutorialJanePoses
		{
			NORMAL,
			THINKING,
			POINTING,
			THUMBSUP,
			KISS,
		}
    }
}