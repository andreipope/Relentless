using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class SpellBoardCard : BoardCard
    {
        public SpellBoardCard(GameObject selfObject)
            : base(selfObject)
        {
        }

        public override void Init(WorkingCard card)
        {
            base.Init(card);
        }

        public override void Init(Card card, int amount = 0)
        {
            base.Init(card, amount);
        }
    }
}
