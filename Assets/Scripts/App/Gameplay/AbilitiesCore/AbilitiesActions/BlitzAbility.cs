namespace Loom.ZombieBattleground
{
    internal class BlitzAbility : CardAbility
    {
        public override void DoAction()
        {
            UnitModelOwner.ApplyBuff(Common.Enumerators.BuffType.BLITZ);
        }
    }
}
