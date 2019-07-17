using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class ExtraGooIfUnitInPlayAbility : AbilityBase
    {
        private const int MaxExtraGooValue = 9999;
        private const int MinExtraGooValue = 0;

        private bool _gooWasChanged;

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
            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action(new object[] { AbilityUnitOwner.OwnerPlayer, 1 });
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (_gooWasChanged)
            {
                Action(new object[] { AbilityUnitOwner.OwnerPlayer, -1 });
            }
        }

        public override void Deactivate()
        {
            base.Deactivate();

            if (_gooWasChanged)
            {
                Action(new object[] { AbilityUnitOwner.OwnerPlayer, -1 });
            }
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

            _gooWasChanged = revertSymbol > 0;

            player.ExtraGoo = Mathf.Clamp(player.ExtraGoo + (Value * revertSymbol), MinExtraGooValue, MaxExtraGooValue);

            if (GameplayManager.CurrentTurnPlayer == player)
            {
                player.CurrentGoo += (Value * revertSymbol);
            }
        }
    }
}
