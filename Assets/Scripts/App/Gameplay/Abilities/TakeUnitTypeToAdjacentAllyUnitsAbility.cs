using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class TakeUnitTypeToAdjacentAllyUnitsAbility : AbilityBase
    {
        public Enumerators.CardType CardType;

        public TakeUnitTypeToAdjacentAllyUnitsAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            CardType = ability.TargetCardType;
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

            Player opponent = GetOpponentOverlord();
            object caller = AbilityUnitOwner != null?AbilityUnitOwner:(object)BoardSpell;

            int targetIndex = -1;
            for (int i = 0; i < opponent.BoardCards.Count; i++)
            {
                if (opponent.BoardCards[i] == TargetUnit)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex > -1)
            {
                if (targetIndex - 1 > -1)
                {
                    TakeTypeToUnit(opponent.BoardCards[targetIndex - 1]);
                }

                if (targetIndex + 1 < opponent.BoardCards.Count)
                {
                    TakeTypeToUnit(opponent.BoardCards[targetIndex + 1]);
                }
            }
        }

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        protected override void UnitOnAttackEventHandler(object info, int damage, bool isAttacker)
        {
            base.UnitOnAttackEventHandler(info, damage, isAttacker);
        }

        private void TakeTypeToUnit(BoardUnit unit)
        {
            if (unit == null)
                return;

            // implement functionality for animations
            switch (CardType)
            {
                case Enumerators.CardType.Heavy:
                    unit.HasHeavy = true;
                    break;
                case Enumerators.CardType.Feral:
                    unit.HasFeral = true;
                    break;
            }
        }
    }
}
