using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ExtraGooIfUnitInPlayAbility : AbilityBase
    {
        private const int MaxExtraGooValue = 9999;
        private const int MinExtraGooValue = 0;

        public int Value { get; }

        public ExtraGooIfUnitInPlayAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            Action(new object[] { PlayerCallerOfAbility, 1 });
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            Action(new object[] { PlayerCallerOfAbility, -1 });
        }

        public override void Deactivate()
        {
            base.Deactivate();

            Action(new object[] { PlayerCallerOfAbility, -1 });
        }

        protected override void PlayerOwnerHasChanged(Player oldPlayer, Player newPlayer)
        {
            Action(new object[] { oldPlayer, -1 });
            Action(new object[] { newPlayer, 1 });
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            Player player = ((object[])info)[0] as Player;
            int revertSymbol = (int)((object[])info)[1];

            player.ExtraGoo = Mathf.Clamp(player.ExtraGoo + (Value * revertSymbol), MinExtraGooValue, MaxExtraGooValue);
            player.CurrentGoo += (Value * revertSymbol);
        }
    }
}
