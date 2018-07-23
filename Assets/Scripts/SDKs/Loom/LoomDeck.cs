
using System;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Loom.Unity3d.Zb;
using UnityEngine;
using Deck = LoomNetwork.CZB.Data.Deck;
using ZbDeck = Loom.Unity3d.Zb.Deck;

public partial class LoomManager
{
    private const string GetDeckDataMethod = "ListDecks";
    private const string DeleteDeckMethod = "DeleteDeck";
    private const string AddDeckMethod = "CreateDeck";
    private const string EditDeckMethod = "EditDeck";
    
    public async Task<ListDecksResponse> GetDecks(string userId)
    {
        if (_contract == null)
            await Init();
        
        var request = new ListDecksRequest {
            UserId = userId
        };
        
        return await _contract.StaticCallAsync<ListDecksResponse>(GetDeckDataMethod, request);
    }

    public async Task DeleteDeck(string userId, string deckId, Action<string> errorResult)
    {
        if (_contract == null)
            await Init();
        
        var request = new DeleteDeckRequest {
            UserId = userId,
            DeckName = deckId
        };
        
        try
        {
            await _contract.CallAsync(DeleteDeckMethod, request);
            errorResult?.Invoke(string.Empty);
        }
        catch (Exception ex)
        {
            //Debug.Log("Exception = " + ex);
            errorResult?.Invoke(ex.ToString());
        }
    }

    public async Task EditDeck(string userId, Deck deck, Action<string> errorResult)
    {
        if (_contract == null)
            await Init();
        
        var cards = new RepeatedField<CardCollection>();
            
        for (var i = 0; i < deck.cards.Count; i++)
        {
            var cardInCollection = new CardCollection
            {
                CardId = deck.cards[i].cardId,
                Amount = deck.cards[i].amount
            };
            Debug.Log("Card in collection = " + cardInCollection.CardId + " , " + cardInCollection.Amount);
            cards.Add(cardInCollection);
        }
        
        var request = new EditDeckRequest
        {
            UserId = userId,
            Deck = new ZbDeck
            {
                Name = deck.name,
                HeroId = deck.heroId,
                Cards = {cards}
            }
        };
        
        try
        {
            await _contract.CallAsync(EditDeckMethod, request);
            errorResult?.Invoke(string.Empty);
        }
        catch (Exception ex)
        {
            //Debug.Log("Exception = " + ex);
            errorResult?.Invoke(ex.ToString());
        }
    }

    public async Task AddDeck(string userId, Deck deck, Action<string> errorResult)
    {
        if (_contract == null)
            await Init();
        
        var cards = new RepeatedField<CardCollection>();
            
        for (var i = 0; i < deck.cards.Count; i++)
        {
            var cardInCollection = new CardCollection
            {
                CardId = deck.cards[i].cardId,
                Amount = deck.cards[i].amount
            };
            Debug.Log("Card in collection = " + cardInCollection.CardId + " , " + cardInCollection.Amount);
            cards.Add(cardInCollection);
        }
            
        var request = new CreateDeckRequest
        {
            UserId = userId,
            Deck = new ZbDeck
            {
                Name = deck.name,
                HeroId = deck.heroId,
                Cards = {cards}
            }
        };

        try
        {
            await _contract.CallAsync(AddDeckMethod, request);
            errorResult?.Invoke(string.Empty);
        }
        catch (Exception ex)
        {
            //Debug.Log("Exception = " + ex);
            errorResult?.Invoke(ex.ToString());
        }
        
    }
    
    
}
