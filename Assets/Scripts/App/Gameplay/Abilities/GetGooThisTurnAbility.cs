using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GetGooThisTurnAbility : AbilityBase
    {
        public int Count;

        public GetGooThisTurnAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Enumerators.AffectObjectType.Card);

            if (AbilityCallType != Enumerators.AbilityCallType.ENTRY)
                return;

            AddGooOnThisTurn();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.DEATH)
                return;

            AddGooOnThisTurn();
        }

        protected override void TurnEndedHandler()
        {
            base.TurnEndedHandler();

            if (AbilityCallType != Enumerators.AbilityCallType.END)
                return;

            AddGooOnThisTurn();
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);
            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            AddGooOnThisTurn();
        }

        private void AddGooOnThisTurn()
        {
            PlayerCallerOfAbility.CurrentGoo = Mathf.Clamp(PlayerCallerOfAbility.CurrentGoo + Count, 0, Constants.MaximumPlayerGoo);
        }
    }
}
