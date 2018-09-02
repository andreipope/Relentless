namespace LoomNetwork.CZB.Common
{
    public class Enumerators
    {
        public enum AbilityActivityType
        {
            Passive,
            Active
        }

        public enum AbilityCallType
        {
            Turn,
            Entry,
            End,
            Attack,
            Death,
            Permanent,
            GotDamage,
            AtDefence,
            InHand
        }

        public enum AbilityEffectType
        {
            None,
            MassiveWaterWave,
            MassiveFire,
            MassiveLightning,
            MassiveToxicAll,
            TargetRock,
            TargetFire,
            TargetLife,
            TargetToxic,
            TargetWater,
            TargetAdjustmentsBomb,
            StunFreezes,
            StunOrDamageFreezes,
            TargetAdjustmentsAir,
            HealDirectly,
            Heal
        }

        public enum AbilityTargetType
        {
            None,
            Player,
            PlayerCard,
            PlayerAllCards,
            Opponent,
            OpponentCard,
            OpponentAllCards,
            AllCards,
            All
        }

        public enum AbilityType
        {
            Heal,
            ModificatorStats,
            ChangeStat,
            Stun,
            StunOrDamageAdjustments,
            Spurt,
            AddGooVial,
            AddGooCarrier,
            Dot,
            Summon,
            SpellAttack,
            MassiveDamage,
            DamageTargetAdjustments,
            DamageTarget,
            CardReturn,
            Weapon,
            ChangeStatOfCreaturesByType,
            AttackNumberOfTimesPerTurn,
            DrawCard,
            DevourZombiesAndCombineStats,
            DestroyUnitByType,
            LowerCostOfCardInHand,
            OverflowGoo,
            LoseGoo,
            DisableNextTurnGoo,
            Rage,
            FreezeUnits,
            TakeDamageRandomEnemy,
            TakeControlEnemyUnit,
            Guard,
            DestroyFrozenUnit,
            UseAllGooToIncreaseStats,
            FirstUnitInPlay,
            AllyUnitsOfTypeInPlayGetStats,
            DamageEnemyUnitsAndFreezeThem,
            ReturnUnitsOnBoardToOwnersDecks,
            TakeUnitTypeToAdjacentAllyUnits,
            EnemyThatAttacksBecomeFrozen,
            TakeUnitTypeToAllyUnit,
            ReviveDiedUnitsOfTypeFromMatch,
            ChangeStatUntillEndOfTurn,
            AttackOverlord,
            AdjacentUnitsGetHeavy,
            FreezeNumberOfRandomAlly,
            AddCardByNameToHand,
            DealDamageToThisAndAdjacentUnits,
            Swing,
            TakeDefenseIfOverlordHasLessDefenseThan,
            GainNumberOfLifeForEachDamageThisDeals,
            AdditionalDamageToHeavyInAttack,
            UnitWeapon,
            TakeDamageAtEndOfTurnToThis,
            DelayedLoseHeavyGainAttack,
            DelayedGainAttack,
            ReanimateUnit,
            PriorityAttack,
            DestroyTargetUnitAfterAttack,
            CostsLessIfCardTypeInHand,
            ReturnUnitsOnBoardToOwnersHands
        }

        public enum ActionType
        {
            AttackPlayerByCreature,
            AttackCreatureByCreature,
            AttackCreatureBySkill,
            AttackPlayerBySkill,
            HealPlayerBySkill,
            HealCreatureBySkill,
            AttackCreatureByAbility,
            AttackPlayerByAbility,
            HealPlayerByAbility,
            HealCreatureByAbility,
            PlayUnitCard,
            PlaySpellCard,
            StunCreatureByAbility,
            StunUnitBySkill,
            SummonUnitCard,
            ReturnToHandCardAbility,
            ReturnToHandCardSkill,
            DrawCardSkill,
            StunPlayerBySkill,
            ReanimateUnitByAbility
        }

        public enum AffectObjectType
        {
            None,
            Player,
            Card,
            Character
        }

        public enum AiActionType
        {
            Test,
            Test2
        }

        public enum AiType
        {
            BlitzAi,
            DefenseAi,
            MixedAi,
            MixedBlitzAi,
            TimeBlitzAi,
            MixedDefenseAi
        }

        public enum AppState
        {
            None,
            AppInit,
            Login,
            MainMenu,
            HeroSelection,
            DeckSelection,
            Collection,
            Shop,
            Gameplay,
            DeckEditing,
            PackOpener,
            Credits
        }

        public enum AttackInfoType
        {
            Any,
            OnlyDifferent
        }

        public enum BuffActivityType
        {
            OneTime,
            Permanent,
            TillFirstDefenseFromAttack,
            TurnBased
        }

        public enum BuffType
        {
            Guard,
            Defence,
            Heavy,
            Weapon,
            Rush,
            Attack,
            Freeze,
            Damage,
            HealAlly,
            Destroy,
            Reanimate
        }

        public enum ButtonState
        {
            Active,
            Default
        }

        public enum CacheDataType
        {
            CardsLibraryData,
            HeroesData,
            CollectionData,
            DecksData,
            DecksOpponentData,
            UserLocalData,
            OpponentActionsLibraryData,
            CreditsData,
            BuffsTooltipData
        }

        public enum CardKind
        {
            Creature,
            Spell
        }

        public enum CardPackType
        {
            Default
        }

        public enum CardRank
        {
            Minion,
            Officer,
            Commander,
            General
        }

        public enum CardSoundType
        {
            None,
            Attack,
            Death,
            Play
        }

        public enum CardType
        {
            Walker,
            Feral,
            Heavy,
            None
        }

        public enum CardZoneOnBoardType
        {
            Deck,
            Graveyard
        }

        public enum EffectActivateType
        {
            PlaySkillEffect
        }

        public enum EndGameType
        {
            Win,
            Lose,
            Cancel
        }

        public enum FadeState
        {
            Default,
            Faded
        }

        public enum GameEndCondition
        {
            Life,
            Time,
            Turn
        }

        public enum InputType
        {
            Keyboard = 0,
            Mouse,
            Touch
        }

        public enum Language
        {
            None,
            De,
            En,
            Ru
        }

        public enum MatchType
        {
            Local,
            Pvp,
            Pve
        }

        public enum MouseCode
        {
            LeftMouseButton = 0,
            RightMouseButton,
            WheelMouse,
            Other
        }

        public enum NotificationButtonState
        {
            Active,
            Inactive
        }

        public enum NotificationType
        {
            Log,
            Error,
            Warning,
            Message
        }

        public enum OverlordSkill
        {
            None,

            // AIR
            Push,
            Draw,
            WindShield,
            WindWall,
            Retreat,

            // EARTH
            Harden,
            StoneSkin,
            Fortify,
            Phalanx,
            Fortress,

            // FIRE
            FireBolt,
            Rabies,
            Fireball,
            MassRabies,
            MeteorShower,

            // LIFE
            HealingTouch,
            Mend,
            Ressurect,
            Enhance,
            Reanimate,

            // TOXIC
            PoisonDart,
            ToxicPower,
            Breakout,
            Infect,
            Epidemic,

            // WATER
            Freeze,
            IceBolt,
            IceWall,
            Shatter,
            Blizzard
        }

        public enum ScreenOrientationMode
        {
            Portrait,
            Landscape
        }

        public enum SetType
        {
            Fire,
            Water,
            Earth,
            Air,
            Life,
            Toxic,
            Item,
            Others,
            None
        }

        public enum SkillTargetType
        {
            None,
            Player,
            PlayerCard,
            PlayerAllCards,
            Opponent,
            OpponentCard,
            OpponentAllCards,
            AllCards
        }

        public enum SkillType
        {
            Primary,
            Secondary
        }

        public enum SoundType
        {
            Click,
            Other,
            Background,
            Battleground,
            Tutorial,
            Cards,
            EndTurn,
            OverlordAbilities,
            Spells,
            WalkerArrival,
            FeralArrival,
            HeavyArrival,
            FeralAttack,
            HeavyAttack1,
            HeavyAttack2,
            WalkerAttack1,
            WalkerAttack2,
            HeroDeath,
            LogoAppear,
            CardBattlegroundToTrash,
            CardDeckToHandMultiple,
            CardDeckToHandSingle,
            CardFlyHand,
            CardFlyHandToBattleground,
            ChangeScreen,
            DeckeditingAddCard,
            DeckeditingRemoveCard,
            LostPopup,
            WonPopup,
            WonRewardPopup,
            YourturnPopup,
            ShuttersClosing,
            ShuttersOpen,
            GooOverflowFadeIn,
            GooOverflowFadeLoop,
            GooOverflowFadeOut
        }

        public enum SpreadsheetType
        {
            Tutorial
        }

        public enum StatType
        {
            Health,
            Damage,
            None
        }

        public enum StunType
        {
            None,
            Freeze,
            Disable
        }

        public enum TooltipObjectType
        {
            Rank,
            Ability,
            UnitType,
            Buff
        }

        public enum TutorialJanePoses
        {
            Normal,
            Thinking,
            Pointing,
            ThumbsUp,
            Kiss
        }

        public enum TutorialReportAction
        {
            EndTurn,
            MoveCard,
            AttackCardCard,
            AttackCardHero,
            UseAbility
        }

        public enum UnitStatusType
        {
            None,
            Frozen
        }
    }
}
