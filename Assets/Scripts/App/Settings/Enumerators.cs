﻿namespace GrandDevs.CZB.Common
{
    public class Enumerators
    {
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
            PACK_OPENER
        }

        public enum ButtonState
        {
            ACTIVE,
            DEFAULT
        }

        public enum SoundType : int
        {
            CLICK,
          //  OTHER,
            BACKGROUND,
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
            USER_LOCAL_DATA
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
            FIREBALL,
            HEAL
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
            MODIFICATOR_STATIC_DAMAGE,
            STUN,
            MODIFICATOR_STAT_VERSUS,
            SPURT,
            ADD_GOO_VIAL,
            DOT,
            SUMMON,
            SPELL_ATTACK,
            MASSIVE_DAMAGE,
			DAMAGE_TARGET_ADJUSTMENTS,
			DAMAGE_TARGET,
        }

        public enum AbilityActivityType
        {
            PASSIVE,
            ACTIVE
        }

        public enum AbilityCallType
        {
            EACH_TURN,
            AT_START,
            AT_END,
            PERMANENT,
            AT_ATTACK
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
			ITEMS,
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
    }
}