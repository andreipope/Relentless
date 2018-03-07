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
    }
}