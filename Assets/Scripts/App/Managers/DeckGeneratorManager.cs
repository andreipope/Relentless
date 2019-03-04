using System;
using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class DeckGeneratorManager : IService
    {
        private IDataManager _dataManager;
    
        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
        }
        
        public void Update()
        {

        }

        public void Dispose()
        {

        }
        
        public void GenerateCardsToDeck(Deck deck, CollectionData collectionData)
        {
            Hero hero = _dataManager.CachedHeroesData.Heroes[deck.HeroId];
            Enumerators.SetType setType = hero.HeroElement;
            
            CardSet heroElementSet = SetTypeUtility.GetCardSet(_dataManager, setType);
            List<Card> cards = heroElementSet.Cards.ToList();

            List<CollectionCardData> availableCollectionCardDataList = GetAvailableCollectionCardData(cards, collectionData);

            List<Card> availableCardList = new List<Card>();
            foreach (Card card in cards)
            {
                int amount = GetCardsAmount
                (
                    card.Name,
                    collectionData                    
                );
                
                for(int i=0; i<amount; ++i)
                {
                    Card addedCard = new Card(card);
                    availableCardList.Add(addedCard);
                }
            }

            RemoveAllCardsFromDeck(deck);
            
            while(deck.GetNumCards() < Constants.DeckMaxSize)
            {
                int randomIndex = Random.Range(0, availableCardList.Count);
                Card card = availableCardList[randomIndex];
                deck.AddCard(card.Name);
                availableCardList.Remove(card);
            }
        }
        
        private List<CollectionCardData> GetAvailableCollectionCardData(List<Card> cards, CollectionData collectionData)
        {
            List<CollectionCardData> availableCardList = new List<CollectionCardData>();
            foreach(Card card in cards)
            {
                CollectionCardData item = new CollectionCardData();
                item.CardName = card.Name;
                item.Amount = GetCardsAmount
                (
                    card.Name,
                    collectionData                    
                );
                availableCardList.Add(item);
            }
            return availableCardList;
        }

        private void RemoveAllCardsFromDeck(Deck deck)
        {
            deck.Cards.Clear();
        }

        private int GetCardsAmount(string cardName, CollectionData collectionData)
        {
            return collectionData.GetCardData(cardName).Amount;
        }
    }
}