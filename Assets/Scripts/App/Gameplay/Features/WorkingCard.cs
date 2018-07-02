// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/



using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoomNetwork.CZB
{
    public class WorkingCard
    {
        public int cardId;
        public Card libraryCard;

        public Player owner;

        public int instanceId;


        public int initialHealth,
                   initialDamage,
                   health,
                   damage;

        public Enumerators.CardType type;

        public bool IsPlayable { get; set; }


        public WorkingCard(Card card, Player player)
        {
            libraryCard = card;
            cardId = card.id;
            owner = player;

            initialHealth = card.health;
            initialDamage = card.damage;
            health = initialHealth;
            damage = initialDamage;

            type = libraryCard.cardType;

            instanceId = GameClient.Get<IGameplayManager>().GetController<CardsController>().GetNewCardInstanceId();
        }
    }
}
