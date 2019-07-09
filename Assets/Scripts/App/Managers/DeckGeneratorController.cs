using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
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

        private INetworkActionManager _networkActionManager;

        public Action<bool, Deck> FinishAddDeck,
                                  FinishEditDeck,
                                  FinishDeleteDeck;

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
            _analyticsManager = GameClient.Get<IAnalyticsManager>();
            _networkActionManager = GameClient.Get<INetworkActionManager>();
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

        public async void ProcessAddDeck(Deck deck)
        {
            GameClient.Get<IUIManager>().DrawPopup<LoadingOverlayPopup>("Saving Deck . . .");

            bool success = false;
            try
            {
                await _networkActionManager.EnqueueNetworkTask(async () =>
                    {
                        long newDeckId = await _backendFacade.AddDeck(_backendDataControlMediator.UserDataModel.UserId, deck);
                        deck.Id = new DeckId(newDeckId);
                        _dataManager.CachedDecksData.Decks.Add(deck);
                        _analyticsManager.SetEvent(AnalyticsManager.EventDeckCreated);
                        Log.Info(" ====== Add Deck " + newDeckId + " Successfully ==== ");

                        if (GameClient.Get<ITutorialManager>().IsTutorial)
                        {
                            _dataManager.CachedUserLocalData.TutorialSavedDeck = deck;
                            await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                        }

                        success = true;

                        _dataManager.CachedUserLocalData.LastSelectedDeckId =  deck.Id;
                        await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);

                        GameClient.Get<ITutorialManager>().ReportActivityAction(Enumerators.TutorialActivityAction.HordeSaved);
                    },
                    keepCurrentAppState: true,
                    onUnknownExceptionCallbackFunc: exception =>
                    {
                        OpenAlertDialog("Not able to Add Deck: \n" + exception.Message);
                        return Task.CompletedTask;
                    }
                );
            }
            catch
            {
                // No additional handling
            }
            finally
            {
                GameClient.Get<IUIManager>().HidePopup<LoadingOverlayPopup>();
                FinishAddDeck?.Invoke(success, deck);
            }
        }

        public async void ProcessEditDeck(Deck deck)
        {
            GameClient.Get<IUIManager>().DrawPopup<LoadingOverlayPopup>("Saving Deck . . .");
            bool success = false;
            try
            {
                await _networkActionManager.EnqueueNetworkTask(async () =>
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

                        _dataManager.CachedUserLocalData.LastSelectedDeckId = deck.Id;
                        await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);
                    },
                    keepCurrentAppState: true,
                    onUnknownExceptionCallbackFunc: exception =>
                    {
                        string message = exception.Message;

                        string[] description = exception.Message.Split('=');
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
                        return Task.CompletedTask;
                    }
                );
            }
            catch
            {
                // No additional handling
            }
            finally
            {
                GameClient.Get<IUIManager>().HidePopup<LoadingOverlayPopup>();
                FinishEditDeck?.Invoke(success, deck);
            }
        }

        public async Task ProcessDeleteDeck(Deck deck)
        {
            GameClient.Get<IUIManager>().DrawPopup<LoadingOverlayPopup>("Deleting Deck . . .");
            bool success = false;
            try
            {
                await _networkActionManager.EnqueueNetworkTask(async () =>
                    {
                        _dataManager.CachedDecksData.Decks.Remove(deck);
                        _dataManager.CachedUserLocalData.LastSelectedDeckId = new DeckId(-1);
                        await _dataManager.SaveCache(Enumerators.CacheDataType.USER_LOCAL_DATA);

                        await _backendFacade.DeleteDeck(
                            _backendDataControlMediator.UserDataModel.UserId,
                            deck.Id
                        );

                        Log.Info($" ====== Delete Deck {deck.Id} Successfully ==== ");
                        success = true;
                    },
                    keepCurrentAppState: true,
                    onUnknownExceptionCallbackFunc: exception =>
                    {
                        OpenAlertDialog($"Not able to Delete Deck {deck.Id}: " + exception.Message);
                        return Task.CompletedTask;
                    }
                );
            }
            catch
            {
                // No additional handling
            }
            finally
            {
                GameClient.Get<IUIManager>().HidePopup<LoadingOverlayPopup>();
                FinishDeleteDeck?.Invoke(success, deck);
            }
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
            OverlordUserInstance overlord = _dataManager.CachedOverlordData.GetOverlordById(deck.OverlordId);
            Enumerators.Faction faction = overlord.Prototype.Faction;

            // Prioritize cards of the champion's faction, but don't exclude cards from other factions,
            // except the faction the champion is weak against.
            Faction overlordElementSet = SetTypeUtility.GetCardFaction(_dataManager, faction);
            List<Card> creatureCards =
                overlordElementSet
                    .Cards
                    .Where(card => collectionData.Cards.Any(collectionCard => collectionCard.CardKey == card.CardKey))
                    .ToList();
            List<Card> itemCards =
                SetTypeUtility.GetCardFaction(_dataManager, Enumerators.Faction.ITEM)
                    .Cards
                    .Where(card => collectionData.Cards.Any(collectionCard => collectionCard.CardKey == card.CardKey))
                    .ToList();

            List<Card> availableCreatureCardList = new List<Card>();
            foreach (Card card in creatureCards)
            {
                int amount = GetCardsAmount
                (
                    card.CardKey,
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
                    card.CardKey,
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
                deck.AddCard(card.CardKey);
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

            int countCardOneToThreeCost = 0;
            int countCardFourToSevenCost = 0;
            int countCardEightToTenCost = 0;

            for (int i = 0; i < 10; ++i)
            {
                int cost = i + 1;
                if (cardSortByGooCost[cost].Count > 0)
                {
                    List<Card> cardList = cardSortByGooCost[cost];
                    Card card = cardList[Random.Range(0, cardList.Count)];
                    cardsToAdd.Add(card);
                    cardList.Remove(card);
                    creatureCardList.Remove(card);

                    if(cost >= 1 && cost <= 3 )
                    {
                        ++countCardOneToThreeCost;
                    }
                    else if(cost >= 4 && cost <= 7 )
                    {
                        ++countCardFourToSevenCost;
                    }
                    else if(cost >= 8 && cost <= 10 )
                    {
                        ++countCardEightToTenCost;
                    }
                }
            }

            List<Card> cardZeroToThreeCostList = cardSortByGooCost[0]
                                          .Concat(cardSortByGooCost[1])
                                          .Concat(cardSortByGooCost[2])
                                          .Concat(cardSortByGooCost[3])
                                          .ToList();

            for (int i = 0; i < 7 - countCardOneToThreeCost && cardZeroToThreeCostList.Count > 0; ++i)
            {
                int randIndex = Random.Range(0, cardZeroToThreeCostList.Count);
                Card card = cardZeroToThreeCostList[randIndex];
                cardsToAdd.Add(card);
                creatureCardList.Remove(card);
                cardZeroToThreeCostList.Remove(card);
                cardSortByGooCost[card.Cost].Remove(card);
            }

            List<Card> cardFourToSevenCostList = cardSortByGooCost[4]
                                          .Concat(cardSortByGooCost[5])
                                          .Concat(cardSortByGooCost[6])
                                          .Concat(cardSortByGooCost[7])
                                          .ToList();

            for (int i = 0; i < 12 - countCardFourToSevenCost && cardFourToSevenCostList.Count > 0; ++i)
            {
                int randIndex = Random.Range(0, cardFourToSevenCostList.Count);
                Card card = cardFourToSevenCostList[randIndex];
                cardsToAdd.Add(card);
                creatureCardList.Remove(card);
                cardFourToSevenCostList.Remove(card);
                cardSortByGooCost[card.Cost].Remove(card);
            }

            List<Card> cardEightToTenCostList = cardSortByGooCost[8]
                                          .Concat(cardSortByGooCost[9])
                                          .Concat(cardSortByGooCost[10])
                                          .ToList();

            for (int i = 0; i < 4 - countCardEightToTenCost && cardEightToTenCostList.Count > 0; ++i)
            {
                int randIndex = Random.Range(0, cardEightToTenCostList.Count);
                Card card = cardEightToTenCostList[randIndex];
                cardsToAdd.Add(card);
                creatureCardList.Remove(card);
                cardEightToTenCostList.Remove(card);
                cardSortByGooCost[card.Cost].Remove(card);
            }

            for (int i = 0; i < 7; ++i)
            {
                if (itemCardList.Count == 0)
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
                deck.AddCard(card.CardKey);
            }
        }

        private List<CollectionCardData> GetAvailableCollectionCardData(List<Card> cards, CollectionData collectionData)
        {
            List<CollectionCardData> availableCardList = new List<CollectionCardData>();
            foreach(Card card in cards)
            {
                CollectionCardData item = new CollectionCardData(
                    card.CardKey,
                    GetCardsAmount(card.CardKey, collectionData)
                );
                availableCardList.Add(item);
            }
            return availableCardList;
        }

        private void RemoveAllCardsFromDeck(Deck deck)
        {
            deck.Cards.Clear();
        }

        private int GetCardsAmount(CardKey cardKey, CollectionData collectionData)
        {
            return collectionData.GetCardData(cardKey).Amount;
        }

        public void OpenAlertDialog(string msg)
        {
            GameClient.Get<ISoundManager>().PlaySound(Enumerators.SoundType.CHANGE_SCREEN, Constants.SfxSoundVolume,
                false, false, true);
            GameClient.Get<IUIManager>().DrawPopup<WarningPopup>(msg);
        }
    }
}
