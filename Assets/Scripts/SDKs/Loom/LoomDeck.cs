
using System;
using System.Threading.Tasks;
using Loom.Google.Protobuf.Collections;
using LoomNetwork.CZB.Protobuf;
using UnityEngine;
using Deck = LoomNetwork.CZB.Data.Deck;
using ZbDeck = LoomNetwork.CZB.Protobuf.Deck;

public partial class LoomManager
{
    private const string GetDeckDataMethod = "ListDecks";
    private const string DeleteDeckMethod = "DeleteDeck";
    private const string AddDeckMethod = "CreateDeck";
    private const string EditDeckMethod = "EditDeck";
    
    public async Task<ListDecksResponse> GetDecks(string userId)
    {
        var request = new ListDecksRequest {
            UserId = userId
        };
        
        return await Contract.StaticCallAsync<ListDecksResponse>(GetDeckDataMethod, request);
    }

    public async Task DeleteDeck(string userId, long deckId)
    {
        var request = new DeleteDeckRequest {
            UserId = userId,
            DeckId = deckId
        };
        
        await Contract.CallAsync(DeleteDeckMethod, request);
    }

    public async Task EditDeck(string userId, Deck deck) {
        EditDeckRequest request = EditDeckRequest(userId, deck);

        await Contract.CallAsync(EditDeckMethod, request);
    }

    private static EditDeckRequest EditDeckRequest(string userId, Deck deck) {
        var cards = new RepeatedField<CardCollection>();

        for (var i = 0; i < deck.cards.Count; i++)
        {
            var cardInCollection = new CardCollection
            {
                CardName = deck.cards[i].cardName,
                Amount = deck.cards[i].amount
            };
            Debug.Log("Card in collection = " + cardInCollection.CardName + " , " + cardInCollection.Amount);
            cards.Add(cardInCollection);
        }

        var request = new EditDeckRequest
        {
            UserId = userId,
            Deck = new ZbDeck
            {
                Id = deck.id,
                Name = deck.name,
                HeroId = deck.heroId,
                Cards = { cards }
            }
        };
        return request;
    }

    public async Task<long> AddDeck(string userId, Deck deck)
    {
        var cards = new RepeatedField<CardCollection>();
            
        for (var i = 0; i < deck.cards.Count; i++)
        {
            var cardInCollection = new CardCollection
            {
                CardName = deck.cards[i].cardName,
                Amount = deck.cards[i].amount
            };
            Debug.Log("Card in collection = " + cardInCollection.CardName + " , " + cardInCollection.Amount);
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

        return (await Contract.CallAsync<CreateDeckResponse>(AddDeckMethod, request)).DeckId;
    }
}
