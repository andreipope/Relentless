using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class SwingAbility : AbilityBase
    {
        public int Value { get; }

        private int _targetIndex;

        private BoardUnitModel _unit;

        public SwingAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Value = ability.Value;
        }

        public override void Activate()
        {
            base.Activate();

            AbilitiesController.ThrowUseAbilityEvent(MainWorkingCard, new List<BoardObject>(), AbilityData.AbilityType, Protobuf.AffectObjectType.Character);
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            _unit = (BoardUnitModel) info;
           
            _targetIndex = -1;
            for (int i = 0; i < _unit.OwnerPlayer.BoardCards.Count; i++)
            {
                if (_unit.OwnerPlayer.BoardCards[i].Model == _unit)
                {
                    _targetIndex = i;
                    break;
                }
            }

            if (_targetIndex > -1)
            {
                InvokeActionTriggered(info);
            }
        }

        protected override void UnitAttackedHandler(BoardObject info, int damage, bool isAttacker)
        {
            base.UnitAttackedHandler(info, damage, isAttacker);

            if (AbilityCallType != Enumerators.AbilityCallType.ATTACK || !isAttacker)
                return;

            if (info is BoardUnitModel)
            {
                Action(info);
            }
        }

        private void TakeDamageToUnit(BoardUnitView unit)
        {
            BattleController.AttackUnitByAbility(AbilityUnitOwner, AbilityData, unit.Model);
        }

        protected override void VFXAnimationEndedHandler()
        {
            base.VFXAnimationEndedHandler();

            List<BoardObject> targets = new List<BoardObject>();

            if (_targetIndex - 1 > -1)
            {
                targets.Add(_unit.OwnerPlayer.BoardCards[_targetIndex - 1].Model);
                TakeDamageToUnit(_unit.OwnerPlayer.BoardCards[_targetIndex - 1]);
            }

            if (_targetIndex + 1 < _unit.OwnerPlayer.BoardCards.Count)
            {
                targets.Add(_unit.OwnerPlayer.BoardCards[_targetIndex + 1].Model);
                TakeDamageToUnit(_unit.OwnerPlayer.BoardCards[_targetIndex + 1]);
            }
        }
    }
}
