using Loom.ZombieBattleground.Data;

namespace Loom.ZombieBattleground
{
    public interface IAbility
    {
        NewAbilityData AbilityData { get; }

        void Init(NewAbilityData data, BoardObject boardObjectOwner);
        void CallAction(object target = null);
    }
}
