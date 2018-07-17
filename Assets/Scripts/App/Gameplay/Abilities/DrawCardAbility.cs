// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LoomNetwork.CZB.Common;
using UnityEngine;
using LoomNetwork.CZB.Data;

namespace LoomNetwork.CZB
{
    public class DrawCardAbility : AbilityBase
    {
        public Enumerators.SetType setType;

        public DrawCardAbility(Enumerators.CardKind cardKind, AbilityData ability) : base(cardKind, ability)
        {
            this.setType = ability.abilitySetType;
        }

        public override void Activate()
        {
            base.Activate();

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

        protected override void OnInputEndEventHandler()
        {
            base.OnInputEndEventHandler();
        }

        public override void Action(object info = null)
        {
            base.Action(info);

            if ((setType == Enumerators.SetType.NONE) ||
                (setType != Enumerators.SetType.NONE && playerCallerOfAbility.BoardCards.FindAll(x => x.Card.libraryCard.cardSetType == setType).Count > 0))
            {
                _cardsController.AddCardToHand(playerCallerOfAbility);
            }
        }
    }
}