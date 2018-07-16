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
        private IDataManager _dataManager;
        private List<Card> _cardsInPack;

        public Enumerators.CardPackType cardPackType;

        public CardPack()
        {
            Init(Enumerators.CardPackType.DEFAULT);
        }

        public CardPack(Enumerators.CardPackType type)
        {
            Init(type);
        }

        private void Init(Enumerators.CardPackType type)
        {
            _dataManager = GameClient.Get<IDataManager>();

            cardPackType = type;

            _cardsInPack = new List<Card>();
        }


        public List<Card> OpenPack(bool isTemporary = false)
        {
            if (isTemporary)
                GetSpecialCardPack();
            else
            {
                for (int i = 0; i < Constants.CARDS_IN_PACK; i++)
                    _cardsInPack.Add(GenerateNewCard());
            }

            return _cardsInPack;
        }

        private Card GenerateNewCard()
        {
            var rarity = (Enumerators.CardRank)IsChanceFit(0);
            var cards = _dataManager.CachedCardsLibraryData.Cards.FindAll((item) => item.cardRank == rarity && item.cardSetType != Enumerators.SetType.OTHERS);
            Card card = cards[Random.Range(0, cards.Count)];
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
            else
                return rarity;
        }

        // TEMPORARY OR SPECIAL
        private void GetSpecialCardPack()
        {
            var fullColection = _dataManager.CachedCardsLibraryData.Cards.FindAll((item) => item.cardSetType != Enumerators.SetType.OTHERS);

            var legendary = fullColection.FindAll((item) => item.cardRank == Enumerators.CardRank.GENERAL);
            var epic = fullColection.FindAll((item) => item.cardRank == Enumerators.CardRank.COMMANDER);

            _cardsInPack.Add(fullColection[Random.Range(0, fullColection.Count)]);
            _cardsInPack.Add(fullColection[Random.Range(0, fullColection.Count)]);
            _cardsInPack.Add(epic[Random.Range(0, epic.Count)]);
            _cardsInPack.Add(fullColection[Random.Range(0, fullColection.Count)]);
            _cardsInPack.Add(legendary[Random.Range(0, legendary.Count)]);
        }
    }
}