using System;
using Loom.Unity3d.Zb;
using LoomNetwork.CZB.Data;
using Loom.Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using Deck = LoomNetwork.CZB.Data.Deck;

public class LoomDeckTest
{
    [SetUp]
    public async void InitLoom()
    {
        await LoomManager.Instance.CreateContract();
        LoomManager.UserId = "LoomTest";
        await LoomManager.Instance.SignUp(LoomManager.UserId, result => { });
    }
    
    [Test]
    public async void GetDeck()
    {
        ListDecksResponse listDecksResponse = null;
        Assert.IsNull(listDecksResponse);
        listDecksResponse  = await LoomManager.Instance.GetDecks(LoomManager.UserId);
        Assert.IsNotNull(listDecksResponse);
        
        var decksData = JsonConvert.DeserializeObject<DecksData>(listDecksResponse.ToString());
        Assert.IsNotNull(decksData);
        Assert.AreEqual(1, decksData.decks.Count);
        Assert.AreEqual("Default", decksData.decks[0].name);
    }

    [Test]
    public async void GetDeck_Empty_Request()
    {
        ListDecksResponse listDecksResponse = null;
        Assert.IsNull(listDecksResponse);
        listDecksResponse = await LoomManager.Instance.GetDecks(string.Empty);
        Assert.IsNull(listDecksResponse);
    }
    
    [Test]
    public async void GetDeck_Wrong_Request()
    {
        ListDecksResponse listDecksResponse = null;
        Assert.IsNull(listDecksResponse);
        listDecksResponse = await LoomManager.Instance.GetDecks("GauravIsGreatWorkingInLoom");
        Assert.IsNull(listDecksResponse);
    }
    
    [Test]
    public async void GetDeck_User_Have_No_Deck()
    {
        var user = "LoomTest_NoDecks";
        await LoomManager.Instance.SignUp(user, result => { });
        
        ListDecksResponse listDecksResponse = null;
        Assert.IsNull(listDecksResponse);
        
        listDecksResponse = await LoomManager.Instance.GetDecks(user);
        Assert.IsNotNull(listDecksResponse);

        await LoomManager.Instance.DeleteDeck(user, "Default", result => { });

        var newListDecksResponse = await LoomManager.Instance.GetDecks(user);
        Assert.IsNull(newListDecksResponse);
    }

    [Test]
    public async void AddDeck()
    {
        var user = "LoomTest_AddDeck";
        await LoomManager.Instance.SignUp(user, result => { });

        var deck = new Deck
        {
            heroId = 0,
            name = "Gaurav"
        };
        //deck.AddCard(0);
        //deck.AddCard(1);

        var response = string.Empty;
        await LoomManager.Instance.AddDeck(user, deck, result => { response = result; });
        Assert.IsTrue(string.IsNullOrEmpty(response));

    }

    [Test]
    public async void AddDeck_Wrong_User_Request()
    {
        var user = "LoomTest_AddDeck_wrong_user";
        var deck = new Deck
        {
            heroId = 0,
            name = "Gaurav"
        };
        //deck.AddCard(0);
        //deck.AddCard(1);

        var response = string.Empty;
        await LoomManager.Instance.AddDeck(user, deck, result => { response = result; });
        Assert.IsFalse(string.IsNullOrEmpty(response));
    }
    
    [Test]
    public async void AddDeck_Wrong_Deck_Request()
    {
        var user = "LoomTest_AddDeck";
        await LoomManager.Instance.SignUp(user, result => { });

        var deck = new Deck
        {
            heroId = 0,
            name = "Gaurav"
        };
        //deck.AddCard(-10);
        //deck.AddCard(-41);

        var response = string.Empty;
        await LoomManager.Instance.AddDeck(user, deck, result => { response = result; });
        Assert.IsFalse(string.IsNullOrEmpty(response));
    }

    [Test]
    public async void EditDeck()
    {
        var user = "LoomTest_EditDeck";
        await LoomManager.Instance.SignUp(user, result => { });

        var deck = new Deck
        {
            name = "Default"
        };
        //deck.AddCard(0);
        //deck.AddCard(1);

        var response = string.Empty;
        await LoomManager.Instance.EditDeck(user, deck, result => { response = result; });
        Assert.IsTrue(string.IsNullOrEmpty(response));
    }

    [Test]
    public async void EditDeck_Wrong_User_Request()
    {
        var user = "LoomTest_EditDeck_wrong_user";
        var deck = new Deck
        {
            heroId = 0,
            name = "Gaurav"
        };
        //deck.AddCard(0);
        //deck.AddCard(1);

        var response = string.Empty;
        await LoomManager.Instance.EditDeck(user, deck, result => { response = result; });
        Assert.IsFalse(string.IsNullOrEmpty(response));
    }
    
    [Test]
    public async void EditDeck_Wrong_Deck_Request()
    {
        var user = "LoomTest_EditDeck";
        await LoomManager.Instance.SignUp(user, result => { });

        var deck = new Deck
        {
            heroId = 0,
            name = "GauravRandomDeck"
        };
        //deck.AddCard(-10);
        //deck.AddCard(-41);

        var response = string.Empty;
        await LoomManager.Instance.EditDeck(user, deck, result => { response = result; });
        Assert.IsFalse(string.IsNullOrEmpty(response));
    }
    

    [Test]
    public async void DeleteDeck()
    {
        var user = "LoomTest_DeleteDeck";
        await LoomManager.Instance.SignUp(user, result => { });

        var response = string.Empty;
        await LoomManager.Instance.DeleteDeck(user, "Default", result => { response = result; });
        Assert.IsTrue(string.IsNullOrEmpty(response));
        
    }

    [Test]
    public async void DeleteDeck_Wrong_User_Request()
    {
        var user = "LoomTest_DeleteDeck_wrong_User";
        var response = string.Empty;
        await LoomManager.Instance.DeleteDeck(user, "Default", result => { response = result; });
        Assert.IsFalse(string.IsNullOrEmpty(response));
    }
    
    [Test]
    public async void DeleteDeck_Wrong_Deck_Request()
    {
        var user = "LoomTest_DeleteDeck_wrong_Deck";
        await LoomManager.Instance.SignUp(user, result => { });
        
        var response = string.Empty;
        await LoomManager.Instance.DeleteDeck(user, "Default_123", result => { response = result; });
        Assert.IsFalse(string.IsNullOrEmpty(response));
    }
}
