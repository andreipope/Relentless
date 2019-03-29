using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Helpers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class DeckGeneratorController : IController
    {
        private static readonly ILog Log = Logging.GetLog(nameof(HordeSelectionWithNavigationPage));
    
        private IDataManager _dataManager;
        
        private BackendFacade _backendFacade;
        
        private BackendDataControlMediator _backendDataControlMediator;
        
        private IAnalyticsManager _analyticsManager;

        public Action<bool, Deck> FinishAddDeck,
                                  FinishEditDeck,
                                  FinishDeleteDeck;
    
        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
        }
        
        public void Update()
        {

        }

        public void Dispose()
        {

        }
        
        public void ResetAll()
        {

        }
        
        public async void ProcessAddDeck(Deck deck, OverlordModel overlord)
        {
            bool success = false;
            deck.OverlordId = overlord.OverlordId;
            deck.PrimarySkill = overlord.PrimarySkill;
            deck.SecondarySkill = overlord.SecondarySkill;

            try
            {
                long newDeckId = await _backendFacade.AddDeck(_backendDataControlMediator.UserDataModel.UserId, deck);
                deck.Id = newDeckId;
                _dataManager.CachedDecksData.Decks.Add(deck);
                _analyticsManager.SetEvent(AnalyticsManager.EventDeckCreated);
                Log.Info(" ====== Add Deck " + newDeckId + " Successfully ==== ");

                if(GameClient.Get<ITutorialManager>().IsTutorial)
                {
                    _dataManager.CachedUserLocalData.TutorialSavedDeck = deck;
                    await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                }
                success = true;
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogExceptionAsWarning(Log, e);

                if (e is Client.RpcClientException || e is TimeoutException)
                {
                    GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true);
                }
                else
                {
                    OpenAlertDialog("Not able to Add Deck: \n" + e.Message);
                }
            }
            
            if (success)
            {
                _dataManager.CachedUserLocalData.LastSelectedDeckId = (int)deck.Id;
                await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);                

                GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.HordeSaved);
            }
            
            FinishAddDeck?.Invoke(success,deck);
        }
        
        public async void ProcessEditDeck(Deck deck)
        {
            bool success = false;
            try
            {
                await _backendFacade.EditDeck(_backendDataControlMediator.UserDataModel.UserId, deck);

                for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count; i++)
                {
                    if (_dataManager.CachedDecksData.Decks[i].Id == deck.Id)
                    {
                        _dataManager.CachedDecksData.Decks[i] = deck;
                        break;
                    }
                }

                _analyticsManager.SetEvent(AnalyticsManager.EventDeckEdited);
                Log.Info(" ====== Edit Deck Successfully ==== ");
                success = true;
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.LogExceptionAsWarning(Log, e);                

                if (e is Client.RpcClientException || e is TimeoutException)
                {
                    GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true);
                }
                else
                {
                    string message = e.Message;

                    string[] description = e.Message.Split('=');
                    if (description.Length > 0)
                    {
                        message = description[description.Length - 1].TrimStart(' ');
                        message = char.ToUpper(message[0]) + message.Substring(1);
                    }
                    if (GameClient.Get<ITutorialManager>().IsTutorial)
                    {
                        message = Constants.ErrorMessageForConnectionFailed;
                    }
                    OpenAlertDialog("Not able to Edit Deck: \n" + message);
                }
            }

            if (success)
            {
                _dataManager.CachedUserLocalData.LastSelectedDeckId = (int)deck.Id;
                await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
            }

            FinishEditDeck?.Invoke(success, deck);
        }
        
        public async void ProcessDeleteDeck(Deck deck)
        {
            bool success = false;
            try
            {
                _dataManager.CachedDecksData.Decks.Remove(deck);
                _dataManager.CachedUserLocalData.LastSelectedDeckId = -1;
                await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                await _dataManager.SaveCache(Enumerators.CacheDataType.OVERLORDS_DATA);

                await _backendFacade.DeleteDeck(
                    _backendDataControlMediator.UserDataModel.UserId,
                    deck.Id
                );

                Log.Info($" ====== Delete Deck {deck.Id} Successfully ==== ");
                success = true;
            }
            catch (TimeoutException e)
            {
                Helpers.ExceptionReporter.SilentReportException(e);
                Log.Warn("Time out ==", e);
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true);
            }
            catch (Client.RpcClientException e)
            {
                Helpers.ExceptionReporter.SilentReportException(e);
                Log.Warn("RpcException ==", e);
                GameClient.Get<IAppStateManager>().HandleNetworkExceptionFlow(e, true);
            }
            catch (Exception e)
            {
                Helpers.ExceptionReporter.SilentReportException(e);
                Log.Info("Result ===", e);
                OpenAlertDialog($"Not able to Delete Deck {deck.Id}: " + e.Message);
                return;
            }

            FinishDeleteDeck?.Invoke(success,deck);
        }
        
        public bool VerifyDeckName(string deckName, string previousDeckName = null)
        {
            if (string.IsNullOrWhiteSpace(deckName))
            {
                OpenAlertDialog("Saving Deck with an empty name is not allowed.");
                return false;
            }
            
            if(!string.IsNullOrEmpty(previousDeckName))
            {
                if (string.Equals(deckName, previousDeckName))
                    return true;
            }

            List<Deck> deckList =  _dataManager.CachedDecksData.Decks;
            foreach (Deck deck in deckList)
            {
                if (deck.Name.Trim().Equals(deckName.Trim(), StringComparison.InvariantCultureIgnoreCase))
                {
                    OpenAlertDialog("Not able to Edit Deck: \n Deck Name already exists.");
                    return false;
                }
            }
            
            return true;
        }

        public void GenerateCardsToDeck(Deck deck, CollectionData collectionData)
        {
            OverlordModel overlord = _dataManager.CachedOverlordData.Overlords[deck.OverlordId];
            Enumerators.Faction faction = overlord.Faction;
            
            Faction overlordElementSet = SetTypeUtility.GetCardFaction(_dataManager, faction);
            List<Card> creatureCards = overlordElementSet.Cards.ToList();
            List<Card> itemCards = SetTypeUtility.GetCardFaction(_dataManager, Enumerators.Faction.ITEM).Cards.ToList();
            
            List<Card> availableCreatureCardList = new List<Card>();
            foreach (Card card in creatureCards)
            {
                int amount = GetCardsAmount
                (
                    card.Name,
                    collectionData                    
                );
                
                for (int i = 0; i < amount; ++i)
                {
                    Card addedCard = new Card(card);
                    availableCreatureCardList.Add(addedCard);
                }
            }
            
            List<Card> availableItemCardList = new List<Card>();
            foreach (Card card in itemCards)
            {
                int amount = GetCardsAmount
                (
                    card.Name,
                    collectionData                    
                );
                
                for (int i = 0; i < amount; ++i)
                {
                    Card addedCard = new Card(card);
                    availableItemCardList.Add(addedCard);
                }
            }

            MidRangeDeckGenerationStyle(deck, availableCreatureCardList.ToList(), availableItemCardList.ToList());
        }
        
        public string GenerateDeckName()
        {
            int index = _dataManager.CachedDecksData.Decks.Count;
            string newName = "HORDE " + index;
            while (true)
            {
                bool isNameCollide = false;
                for (int i = 0; i < _dataManager.CachedDecksData.Decks.Count; ++i)
                {
                    if (string.Equals(_dataManager.CachedDecksData.Decks[i].Name,newName))
                    {
                        isNameCollide = true;
                        ++index;
                        newName = "HORDE " + index;
                        break;
                    }
                }
                if (!isNameCollide)
                    return newName;
            }
        }
        
        private void BasicDeckGenerationStyle(Deck deck, List<Card> availableCardList)
        {
            RemoveAllCardsFromDeck(deck);
            
            int amountLeftToFill = (int)(Constants.DeckMaxSize - deck.GetNumCards());
            while(amountLeftToFill > 0)
            {
                if (amountLeftToFill > availableCardList.Count)
                    break;
                
                int randomIndex = Random.Range(0, availableCardList.Count);
                Card card = availableCardList[randomIndex];
                deck.AddCard(card.Name);
                availableCardList.Remove(card);
                
                amountLeftToFill = (int)(Constants.DeckMaxSize - deck.GetNumCards());
            }
        }
        
        private void MidRangeDeckGenerationStyle(Deck deck, List<Card> creatureCardList, List<Card> itemCardList)
        {
            RemoveAllCardsFromDeck(deck);

            List<Card> cardsToAdd = new List<Card>();

            List<List<Card>> cardSortByGooCost = new List<List<Card>>();
            for (int i = 0; i < 11; ++i)
            {
                cardSortByGooCost.Add(new List<Card>());
            }
            foreach(Card card in creatureCardList)
            {
                if (card.Kind == Enumerators.CardKind.ITEM)
                     continue;
                     
                int index = Mathf.Clamp(card.Cost, 0, 10);
                cardSortByGooCost[index].Add(card);
            }
            
            for (int i = 0; i < 7; ++i)
            {
                int randCost = Random.Range(1, 4);
                if(cardSortByGooCost[randCost].Count > 0)
                {
                    List<Card> cardList = cardSortByGooCost[randCost];
                    Card card = cardList[Random.Range(0, cardList.Count)];
                    cardsToAdd.Add(card);
                    cardList.Remove(card);
                    creatureCardList.Remove(card);
                }
            }
            
            for (int i = 0; i < 12; ++i)
            {
                int randCost = Random.Range(4, 8);
                if(cardSortByGooCost[randCost].Count > 0)
                {
                    List<Card> cardList = cardSortByGooCost[randCost];
                    Card card = cardList[Random.Range(0, cardList.Count)];
                    cardsToAdd.Add(card);
                    cardList.Remove(card);
                    creatureCardList.Remove(card);
                }
            }
            
            for (int i = 0; i < 4; ++i)
            {
                int randCost = Random.Range(8, 11);
                if(cardSortByGooCost[randCost].Count > 0)
                {
                    List<Card> cardList = cardSortByGooCost[randCost];
                    Card card = cardList[Random.Range(0, cardList.Count)];
                    cardsToAdd.Add(card);
                    cardList.Remove(card);
                    creatureCardList.Remove(card);
                }
            }

            for (int i = 0; i < 7; ++i)
            {
                if (itemCardList.Count < 0)
                    break;
                    
                Card card = itemCardList[Random.Range(0, itemCardList.Count)];
                cardsToAdd.Add(card);
                itemCardList.Remove(card);
            }

            int amountLeftToFill = (int)(Constants.DeckMaxSize - cardsToAdd.Count);
            while(amountLeftToFill > 0)
            {
                if (amountLeftToFill > creatureCardList.Count)
                    break;
                
                int randomIndex = Random.Range(0, creatureCardList.Count);
                Card card = creatureCardList[randomIndex];
                cardsToAdd.Add(card);
                creatureCardList.Remove(card);
                
                amountLeftToFill = (int)(Constants.DeckMaxSize - cardsToAdd.Count);
            }

            cardsToAdd = cardsToAdd.OrderBy(x => x.Faction).ThenBy(x => x.Cost).ToList();
            foreach(Card card in cardsToAdd)
            {
                deck.AddCard(card.Name);
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
        
        public void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            GameClient.Get<IUIManager>().DrawPopup<WarningPopup>(msg);
        }
    }
}
