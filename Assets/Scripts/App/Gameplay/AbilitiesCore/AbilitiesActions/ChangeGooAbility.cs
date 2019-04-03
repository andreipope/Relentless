using Loom.ZombieBattleground.Common;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    internal class ChangeGooAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case Player player:
                        ChangeGooByPlayer(player);
                        break;
                }
            }
        }

        private void ChangeGooByPlayer(Player player)
        {
            if (player == null)
                return;

            if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Goo))
            {
                int value = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                           Common.Enumerators.AbilityParameter.Goo);

                if (AbilitiesController.HasSubTrigger(this, Common.Enumerators.AbilitySubTrigger.GainGooBottle))
                {
                    int gooVials = value;
                    for (int i = 0; i < value; i++)
                    {
                        if (player.GooVials + gooVials >= player.MaxGooVials)
                        {
                            player.PlayerCardsController.AddCardFromDeckToHand();
                            gooVials--;
                        }
                    }

                    player.GooVials += gooVials;
                }
                if (AbilitiesController.HasSubTrigger(this, Common.Enumerators.AbilitySubTrigger.LoseGooBottle))
                {
                    player.CurrentGoo -= value;
                    player.GooVials -= value;
                }
                if (AbilitiesController.HasSubTrigger(this, Common.Enumerators.AbilitySubTrigger.GainGoo))
                {
                    player.CurrentGoo = UnityEngine.Mathf.Clamp(player.CurrentGoo + value, 0, Constants.MaximumPlayerGoo);
                }
                if (AbilitiesController.HasSubTrigger(this, Common.Enumerators.AbilitySubTrigger.LoseGoo))
                {
                    int countOfTurns = 0;
                    if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Turns))
                    {
                        countOfTurns = AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                                   Common.Enumerators.AbilityParameter.Turns);
                    }
                    if (countOfTurns == 0)
                    {
                        player.CurrentGoo -= value;
                    }
                    else
                    {
                        player.CurrentGooModificator -= value;
                    }
                }
                if (AbilitiesController.HasSubTrigger(this, Common.Enumerators.AbilitySubTrigger.OverflowGoo))
                {
                    player.CurrentGoo += value;

                    PostGameActionReport(Enumerators.ActionType.CardAffectingOverlord,
                        new List<PastActionsPopup.TargetEffectParam>()
                        {
                            new PastActionsPopup.TargetEffectParam()
                            {
                                ActionEffectType = Enumerators.ActionEffectType.Overflow,
                                Target = player,
                                HasValue = true,
                                Value = value
                            }
                        });
                }
            }
        }
    }
}
