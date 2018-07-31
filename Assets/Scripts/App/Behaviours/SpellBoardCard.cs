// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/


using UnityEngine;
using TMPro;
using System;

namespace LoomNetwork.CZB
{
    public class SpellBoardCard : BoardCard
    {

        public SpellBoardCard(GameObject selfObject) : base(selfObject)
        {
        }

        public override void Init(WorkingCard card)
        {
            base.Init(card);
        }

        public override void Init(Data.Card card, int amount = 0)
        {
            base.Init(card, amount);
        }
    }
}