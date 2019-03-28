using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Loom.ZombieBattleground
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

            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            Action();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            BoardUnitModel boardUnitModel = null;

            if (PredefinedTargets != null && PredefinedTargets.Count > 0)
            {
                boardUnitModel = PlayerCallerOfAbility.CardsInHand.FirstOrDefault(cardInHand => cardInHand.InstanceId.Id.ToString() == PredefinedTargets[0].Parameters.CardName);
            }

            boardUnitModel = CardsController.LowGooCostOfCardInHand(PlayerCallerOfAbility, boardUnitModel, Value);

            if (boardUnitModel != null)
            {
                InvokeUseAbilityEvent(new List<ParametrizedAbilityBoardObject>()
                {
                    new ParametrizedAbilityBoardObject(PlayerCallerOfAbility,
                        new ParametrizedAbilityParameters()
                        {
                            CardName = boardUnitModel.InstanceId.Id.ToString()
                        })
                }
                );
            }
        }
    }
}
