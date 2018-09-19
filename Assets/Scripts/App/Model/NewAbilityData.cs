using Loom.ZombieBattleground.Common;
using System.Collections.Generic;

namespace Loom.ZombieBattleground.Data
{
    public class NewAbilityData
    {
        public AbilityEnumerator.AbilityTrigger Trigger;
        
        public AbilityEnumerator.AbilityType Type;

        public AbilityEnumerator.AbilityAdjacentEffectMode AdjacentEffectMode;

        public AbilityEnumerator.AbilityTargetSelectMode TargetSelectMode;

        public AbilityEnumerator.AbilityPossibleTargets PossibleTargets;

        public AbilityEnumerator.AbilityTargetTag TargetTag;

        public AbilityEnumerator.AbilityDestination Destination;

        public AbilityEnumerator.AbilityChangeMode ChangeMode;

        public AbilityEnumerator.StatType Stat;

        public List<AbilityRestrictionData> Restrictions;

        public int MaxTargetCount;

        public int Delay;

        public int MouldId;

        public int NumberOfVials;

        public int StatValue;

        public int CardCount;

        public int AttackCount;

        public int GooCostReduction;

        public bool OverrideValueByTargetStat;

        public bool CanOverflow;
    }

    public class AbilityRestrictionData
    {
        public AbilityEnumerator.AbilityRestrictionType Type;

        public AbilityEnumerator.FactionType Faction;

        public AbilityEnumerator.CreatureType CreatureType;

        public AbilityEnumerator.SpecialStatus SpecialStatus;
    }
}
