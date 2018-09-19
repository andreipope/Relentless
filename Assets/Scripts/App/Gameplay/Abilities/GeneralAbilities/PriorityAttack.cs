using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public class PriorityAttack : IAbility
    {
        private BoardObject _abilityOwner;

        public NewAbilityData AbilityData { get; private set; }

        public void Init(NewAbilityData data, BoardObject owner)
        {
            AbilityData = data;
            _abilityOwner = owner;
        }

        public void CallAction(object target)
        {
            if (target != null && target is BoardUnit unit)
            {
                unit.AttackAsFirst = true;
            }
            else
            {
                (_abilityOwner as BoardUnit).AttackAsFirst = true;
            }
        }
    }
}
