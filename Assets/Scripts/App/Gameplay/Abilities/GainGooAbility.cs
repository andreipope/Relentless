using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class GainGooAbility : AbilityBase
    {
        public int Count;

        public GainGooAbility(Enumerators.CardKind cardKind, AbilityData ability)
            : base(cardKind, ability)
        {
            Count = ability.Count;
        }

        public override void Activate()
        {
            base.Activate();

            InvokeUseAbilityEvent();
            if (AbilityTrigger != Enumerators.AbilityTrigger.ENTRY)
                return;

            HandleSubTriggers();
        }

        protected override void UnitDiedHandler()
        {
            base.UnitDiedHandler();

            if (AbilityTrigger != Enumerators.AbilityTrigger.DEATH)
                return;

            HandleSubTriggers();
        }

        private void HandleSubTriggers()
        {
            if (AbilityData.SubTrigger == Enumerators.AbilitySubTrigger.LessDefThanInOpponent)
            {
                if (PlayerCallerOfAbility.Defense < GetOpponentOverlord().Defense)
                {
                    if (PlayerCallerOfAbility.GooVials + Count > PlayerCallerOfAbility.MaxGooVials)
                    {
                        AddCardFromDeckToHand(PlayerCallerOfAbility);
                    }
                    else
                    {
                        GainGoo(PlayerCallerOfAbility, Count);
                    }
                }
            }
            else
            {
                GainGoo(PlayerCallerOfAbility, Count);
            }
        }

        private void AddCardFromDeckToHand(Player player)
        {
            player.PlayerCardsController.AddCardFromDeckToHand();
        }

        private void GainGoo(Player player, int count)
        {
            if (GameplayManager.CurrentTurnPlayer == player)
            {
                player.CurrentGoo = Mathf.Clamp(player.CurrentGoo + count, 0, (int)player.MaxGooVials);
                player.GooVials = Mathf.Clamp(player.GooVials + count, 0, (int)player.MaxGooVials);
            }
            else
            {
                player.GooVials = Mathf.Clamp(player.GooVials + count, 0, (int)player.MaxGooVials);
            }
        }
    }
}
