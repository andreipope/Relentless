using System.Collections.Generic;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public class CardPack
    {
        private IDataManager _dataManager;

        private List<Card> _cardsInPack;

        public CardPack(Enumerators.CardPackType type)
        {
            Init(type);
        }

        public List<Card> OpenPack(bool isTemporary = false)
        {
            if (isTemporary)
            {
                GetSpecialCardPack();
            }
            else
            {
                for (int i = 0; i < Constants.CardsInPack; i++)
                {
                    _cardsInPack.Add(GenerateNewCard());
                }
            }

            return _cardsInPack;
        }

        private void Init(Enumerators.CardPackType type)
        {
            _dataManager = GameClient.Get<IDataManager>();

            _cardsInPack = new List<Card>();
        }

        private Card GenerateNewCard()
        {
            Enumerators.CardRank rarity = (Enumerators.CardRank) IsChanceFit(0);
            List<Card> cards = _dataManager.CachedCardsLibraryData.Cards.FindAll(item =>
                item.CardRank == rarity && item.CardSetType != Enumerators.SetType.OTHERS);
            Card card = cards[Random.Range(0, cards.Count)].Clone();
            return card;
        }

        private int IsChanceFit(int rarity)
        {
            while (true)
            {
                int random = Random.Range(0, 100);
                if (random <= 90)
                    return rarity;

                rarity++;
            }
        }

        // TEMPORARY OR SPECIAL
        private void GetSpecialCardPack()
        {
            List<Card> fullColection =
                _dataManager.CachedCardsLibraryData.Cards.FindAll(item =>
                    item.CardSetType != Enumerators.SetType.OTHERS);

            List<Card> legendary = fullColection.FindAll(item => item.CardRank == Enumerators.CardRank.GENERAL);
            List<Card> epic = fullColection.FindAll(item => item.CardRank == Enumerators.CardRank.COMMANDER);

            _cardsInPack.Add(fullColection[Random.Range(0, fullColection.Count)]);
            _cardsInPack.Add(fullColection[Random.Range(0, fullColection.Count)]);
            _cardsInPack.Add(epic[Random.Range(0, epic.Count)]);
            _cardsInPack.Add(fullColection[Random.Range(0, fullColection.Count)]);
            _cardsInPack.Add(legendary[Random.Range(0, legendary.Count)]);
        }
    }
}
