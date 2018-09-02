// Copyright (c) 2018 - Loom Network. All rights reserved.
// https://loomx.io/

using System.Collections.Generic;
using LoomNetwork.CZB.Common;
using LoomNetwork.CZB.Data;
using UnityEngine;

namespace LoomNetwork.CZB
{
    public class CardPack
    {
        public Enumerators.CardPackType cardPackType;

        private IDataManager _dataManager;

        private List<Card> _cardsInPack;

        public CardPack()
        {
            Init(Enumerators.CardPackType.DEFAULT);
        }

        public CardPack(Enumerators.CardPackType type)
        {
            Init(type);
        }

        public List<Card> OpenPack(bool isTemporary = false)
        {
            if (isTemporary)
            {
                GetSpecialCardPack();
            } else
            {
                for (int i = 0; i < Constants.CARDS_IN_PACK; i++)
                {
                    _cardsInPack.Add(GenerateNewCard());
                }
            }

            return _cardsInPack;
        }

        private void Init(Enumerators.CardPackType type)
        {
            _dataManager = GameClient.Get<IDataManager>();

            cardPackType = type;

            _cardsInPack = new List<Card>();
        }

        private Card GenerateNewCard()
        {
            Enumerators.CardRank rarity = (Enumerators.CardRank)IsChanceFit(0);
            List<Card> cards = _dataManager.CachedCardsLibraryData.Cards.FindAll(item => (item.cardRank == rarity) && (item.cardSetType != Enumerators.SetType.OTHERS));
            Card card = cards[Random.Range(0, cards.Count)].Clone();
            return card;
        }

        private int IsChanceFit(int rarity)
        {
            int random = Random.Range(0, 100);
            if (random > 90)
            {
                rarity++;
                return IsChanceFit(rarity);
            }

            return rarity;
        }

        // TEMPORARY OR SPECIAL
        private void GetSpecialCardPack()
        {
            List<Card> fullColection = _dataManager.CachedCardsLibraryData.Cards.FindAll(item => item.cardSetType != Enumerators.SetType.OTHERS);

            List<Card> legendary = fullColection.FindAll(item => item.cardRank == Enumerators.CardRank.GENERAL);
            List<Card> epic = fullColection.FindAll(item => item.cardRank == Enumerators.CardRank.COMMANDER);

            _cardsInPack.Add(fullColection[Random.Range(0, fullColection.Count)]);
            _cardsInPack.Add(fullColection[Random.Range(0, fullColection.Count)]);
            _cardsInPack.Add(epic[Random.Range(0, epic.Count)]);
            _cardsInPack.Add(fullColection[Random.Range(0, fullColection.Count)]);
            _cardsInPack.Add(legendary[Random.Range(0, legendary.Count)]);
        }
    }
}
