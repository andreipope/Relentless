using static Loom.ZombieBattleground.CardAbilityData;

namespace Loom.ZombieBattleground
{
    internal class DrawAbility : CardAbility
    {
        public override void DoAction()
        {
            foreach (TargetInfo targetInfo in CardAbilityData.Targets)
            {
                switch (targetInfo.Target)
                {
                    case Common.Enumerators.Target.Player:
                        PlayerOwner.PlayerCardsController.AddCardFromDeckToHand();
                        PlayerOwner.PlayDrawCardVFX();
                        break;
                    case Common.Enumerators.Target.Opponent:
                        PlayerOwner.PlayerCardsController.AddCardToHandFromOtherPlayerDeck();
                        break;
                }
            }
        }
    }
}
