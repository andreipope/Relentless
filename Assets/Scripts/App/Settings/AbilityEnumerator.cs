namespace Loom.ZombieBattleground.Common
{
    public class AbilityEnumerator
    {
        public enum AbilityTrigger
        {
            NONE,
            ATTACK,
            DAMAGE,
            DEATH,
            DELAYED,
            END_TURN,
            ENTRY,
            START_TURN,
        }

        public enum AbilityAdjacentEffectMode
        {
            NONE,
            TARGET,
            ADJACENT,
            TARGET_AND_ADJACENT
        }

        public enum AbilityTargetSelectMode
        {
            NONE,
            SELECT,
            ALL,
            RANDOM,
            SELF
        }

        public enum AbilityPossibleTargets
        {
            NONE,
            ALLY_ANY,
            ALLY_CARD,
            ALLY_OVERLORD,
            ANY,
            ANY_CARD,
            ANY_OVERLORD,
            OPPONENT_ANY,
            OPPONENT_CARD,
            OPPONENT_OVERLORD
        }

        public enum AbilityTargetTag
        {
            NONE
        }

        public enum AbilityDestination
        {
            NONE,
            OWN_HAND,
            OWN_BOARD,
            OPPONENT_BOARD
        }

        public enum AbilityChangeMode
        {
            NONE,
            GOO,
            VIALS,
        }

        public enum AbilityType
        {
            ATTACK_NUMBER_OF_TIMES_PER_TURN,
            CHANGE_GOO_VIALS,
            CHANGE_STAT,
            COMBINE_STATS,
            DRAW_CARD,
            LOWER_GOO_COST_OF_CARD_IN_HAND,
            PRIORITY_ATTACK,
            RETURN_UNITS,
            SET_CREATURE_TYPE,
            SET_SPECIAL_STATUS,
            SPAWN_CARD,
            TAKE_CONTROL_ENEMY_UNIT,
            REPLACE_UNITS_WITH_TYPE_ON_STRONGER_ONES,
            COSTS_LESS_IF_CARD_TYPE_IN_HAND
        }

        public enum AbilityRestrictionType
        {
            NONE,
            CREATURE_TYPE,
            FACTION,
            STAT_IS_A_MAX
        }

        public enum SpecialStatus
        {
            DISTRACT,
            FREEZE,
            GUARD
        }

        public enum FactionType
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
            ALLY_OVERLORD,
            ALLY_CARD,
            ALLY_ANY,
            OPPONENT_OVERLORD,
            OPPONENT_CARD,
            OPPONENT_ANY,
            ANY
        }

        public enum StatType
        {
            NONE,
            DEFENCE,
            ATTACK,
        }
    }
}
