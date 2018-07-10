// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



﻿namespace LoomNetwork.CZB.Common
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
            BATTLEGROUND,
            TUTORIAL,
            CARDS,
            END_TURN,

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
            YOURTURN_POPUP

        }

        public enum CardSoundType
        {
            NONE,
            ATTACK,
            DEATH,
            PLAY
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

        public enum CardRank
        {
            MINION,
            OFFICER,
            COMMANDER,
            GENERAL
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
            CHANGE_STAT_OF_CREATURES_BY_TYPE,
            ATTACK_NUMBER_OF_TIMES_PER_TURN
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

        public enum AIActionType
        {
            TEST,
            TEST2
        }

        public enum ActionType
        {
            ATTACK_PLAYER_BY_CREATURE,
            ATTACK_CREATURE_BY_CREATURE,
            ATTACK_CREATURE_BY_SKILL,
            ATTACK_PLAYER_BY_SKILL,
            HEAL_PLAYER_BY_SKILL,
            HEAL_CREATURE_BY_SKILL,
            ATTACK_CREATURE_BY_ABILITY,
            ATTACK_PLAYER_BY_ABILITY,
            HEAL_PLAYER_BY_ABILITY,
            HEAL_CREATURE_BY_ABILITY,
            PLAY_UNIT_CARD,
            PLAY_SPELL_CARD,
            STUN_CREATURE_BY_ABILITY,
            STUN_CREATURE_BY_SKILL,
            SUMMON_UNIT_CARD,
            RETURN_TO_HAND_CARD_ABILITY,
            RETURN_TO_HAND_CARD_SKILL
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

        public enum CardZoneOnBoardType
        {
            DECK,
            GRAVEYARD
        }

        public enum CardPackType
        {
            DEFAULT
        }

        public enum EndGameType
        {
            WIN,
            LOSE,
            CANCEL
        }

        public enum MatchType
        {
            LOCAL,

            PVP,
            PVE
        }

        public enum SkillType
        {
            PRIMARY,
            SECONDARY
        }

        public enum BuffType
        {
            SHIELD,
            DEFENCE,
            HEAVY,
            WEAPON,
            RUSH,
            ATTACK,
            FREEZE,
            DAMAGE,
            HEAL_ALLY
        }

        public enum BuffActivityType
        {
            ONE_TIME,
            PERMANENT,
            TILL_FIRST_DEFENSE_FROM_ATTACK,
            TURN_BASED,
        }

        public enum AttackInfoType
        {
            ANY,
            ONLY_DIFFERENT
        }
    }
}