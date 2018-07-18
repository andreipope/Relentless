
using System;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Loom.Unity3d.Zb;
using LoomNetwork.CZB.Data;
using UnityEngine;

public partial class LoomManager
{
    private const string GetDeckDataMethod = "GetDecks";
    private const string DeleteDeckMethod = "DeleteDeck";
    private const string AddDeckMethod = "AddDeck";
    
    public async Task<UserDecks> GetDecks(string userId)
    {
        if (_contract == null)
            await Init();
        
        var request = new GetDeckRequest {
            UserId = userId
        };
        
        return await _contract.StaticCallAsync<UserDecks>(GetDeckDataMethod, request);
    }

    public async Task DeleteDeck(string userId, string deckId, Action<string> errorResult)
    {
        if (_contract == null)
            await Init();
        
        var request = new DeleteDeckRequest {
            UserId = userId,
            DeckId = deckId
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

    public async Task AddDeck(string userId, Deck deck, Action<string> errorResult)
    {
        if (_contract == null)
            await Init();
        
        var cards = new RepeatedField<CardInCollection>();
            
        for (var i = 0; i < deck.cards.Count; i++)
        {
            var cardInCollection = new CardInCollection();
            cardInCollection.CardId = deck.cards[i].cardId;
            cardInCollection.Amount = deck.cards[i].amount;
            Debug.Log("Card in collection = " + cardInCollection.CardId + " , " + cardInCollection.Amount);
            cards.Add(cardInCollection);
        }
            
        var request = new AddDeckRequest
        {
            UserId = LoomManager.UserId,
            Deck = new ZBDeck
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
