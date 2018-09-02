using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class LowerCostOfCardInHandAbility : AbilityBase
    {
        public int Value;

        public LowerCostOfCardInHandAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            if (AbilityCallType != Enumerators.AbilityCallType.Entry)

                return;

            Action();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            CardsController.LowGooCostOfCardInHand(PlayerCallerOfAbility, null, Value);
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }
    }
}
