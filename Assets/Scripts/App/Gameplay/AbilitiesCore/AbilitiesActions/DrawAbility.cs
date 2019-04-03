using Loom.ZombieBattleground.Common;
using System.Collections.Generic;
using UnityEngine;
using static Loom.ZombieBattleground.CardAbilityData;

namespace Loom.ZombieBattleground
{
    internal class DrawAbility : CardAbility
    {
        public override void DoAction(IReadOnlyList<GenericParameter> genericParameters)
        {
            bool fromSelf = true;
            int count = 1;
            foreach (BoardObject target in Targets)
            {
                switch (target)
                {
                    case Player player:

                        if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Target))
                        {
                            Common.Enumerators.Target targetPlayer = AbilitiesController.GetParameterValue<Common.Enumerators.Target>(GenericParameters,
                                                                          Common.Enumerators.AbilityParameter.Target);

                            fromSelf = targetPlayer == Common.Enumerators.Target.Player;
                        }

                        if (AbilitiesController.HasParameter(GenericParameters, Common.Enumerators.AbilityParameter.Count))
                        {
                            count = Mathf.Clamp(AbilitiesController.GetParameterValue<int>(GenericParameters,
                                                                          Common.Enumerators.AbilityParameter.Count),
                                                                          1,
                                                                          (int)Constants.MaxBoardUnits);
                        }
                        DrawCard(player, count, fromSelf);
                        break;
                }
            }
        }

        private void DrawCard(Player player, int count, bool fromSelf = true)
        {
            for (int i = 0; i < count; i++)
            {
                if(fromSelf)
                {
                    player.PlayerCardsController.AddCardFromDeckToHand();
                }
                else
                {
                    player.PlayerCardsController.AddCardToHandFromOtherPlayerDeck();
                }
            }
            player.PlayDrawCardVFX();
        }
    }
}
