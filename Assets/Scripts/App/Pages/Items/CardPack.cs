using System.Collections.Generic;
using System.Linq;
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
            List<Card> cards =
                _dataManager.CachedCardsLibraryData.Cards
                    .Where(item => item.Rank == rarity && !item.Hidden)
                    .ToList();
            Card card = new Card(cards[Random.Range(0, cards.Count)]);
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
            List<Card> fullCollection =
                _dataManager.CachedCardsLibraryData.Cards
                    .Where(item => !item.Hidden)
                    .ToList();

            List<Card> legendary = fullCollection.FindAll(item => item.Rank == Enumerators.CardRank.GENERAL);
            List<Card> epic = fullCollection.FindAll(item => item.Rank == Enumerators.CardRank.COMMANDER);

            _cardsInPack.Add(fullCollection[Random.Range(0, fullCollection.Count)]);
            _cardsInPack.Add(fullCollection[Random.Range(0, fullCollection.Count)]);
            _cardsInPack.Add(epic[Random.Range(0, epic.Count)]);
            _cardsInPack.Add(fullCollection[Random.Range(0, fullCollection.Count)]);
            _cardsInPack.Add(legendary[Random.Range(0, legendary.Count)]);
        }
    }
}
