using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Loom.Client;
using LoomNetwork.CZB.Protobuf;
using LoomNetwork.CZB.Data;
using Loom.Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Deck = LoomNetwork.CZB.Data.Deck;
using Random = UnityEngine.Random;

public class LoomDeckTest
{
    [SetUp]
    public void Init()
    {
        LoomTestContext.TestSetUp();
    }

    [TearDown]
    public void TearDown()
    {
        LoomTestContext.TestTearDown();
    }

    [UnityTest]
    public IEnumerator GetDeck()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            var user = LoomTestContext.CreateUniqueUserId("LoomTest_GetDeck");
            await LoomTestContext.LoomManager.SignUp(user);
            
            ListDecksResponse listDecksResponse = 
                await LoomTestContext.LoomManager.GetDecks(user);
            Assert.IsNotNull(listDecksResponse);

            var decksData = JsonConvert.DeserializeObject<DecksData>(listDecksResponse.ToString());
            Assert.IsNotNull(decksData);
            Assert.AreEqual(1, decksData.decks.Count);
            Assert.AreEqual("Default", decksData.decks[0].name);
        });
    }

    [UnityTest]
    public IEnumerator GetDeck_Empty_Request()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            var user = LoomTestContext.CreateUniqueUserId("LoomTest_GetDeck");
            await LoomTestContext.LoomManager.SignUp(user);
            Assert.IsNull(await LoomTestContext.LoomManager.GetDecks(string.Empty));
        });
    }

    [UnityTest]
    public IEnumerator GetDeck_Wrong_Request()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            Assert.IsNull(await LoomTestContext.LoomManager.GetDecks("GauravIsGreatWorkingInLoom"));
        });
    }

    [UnityTest]
    public IEnumerator GetDeck_User_Have_No_Deck()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            var user = LoomTestContext.CreateUniqueUserId("LoomTest_NoDecks");
            await LoomTestContext.LoomManager.SignUp(user);

            ListDecksResponse listDecksResponse = null;
            Assert.IsNull(listDecksResponse);

            listDecksResponse = await LoomTestContext.LoomManager.GetDecks(user);
            Assert.IsNotNull(listDecksResponse);

            await LoomTestContext.LoomManager.DeleteDeck(user, 0);

            var newListDecksResponse = await LoomTestContext.LoomManager.GetDecks(user);
            Assert.IsNull(newListDecksResponse);
        });
    }

    [UnityTest]
    public IEnumerator AddDeck()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            var user = LoomTestContext.CreateUniqueUserId("LoomTest_AddDeck");
            await LoomTestContext.LoomManager.SignUp(user);

            var deck = new Deck
            {
                heroId = 0,
                name = "Gaurav"
            };
            //deck.AddCard(0);
            //deck.AddCard(1);

            await LoomTestContext.LoomManager.AddDeck(user, deck);
        });
    }

    [UnityTest]
    public IEnumerator AddDeck_Wrong_User_Request()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            var user = LoomTestContext.CreateUniqueUserId("LoomTest_AddDeck_wrong_user");
            var deck = new Deck
            {
                heroId = 0,
                name = "Gaurav"
            };

            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.LoomManager.AddDeck(user, deck);
            });
        });
    }

    [UnityTest]
    public IEnumerator AddDeck_Wrong_Deck_Request()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            var user = LoomTestContext.CreateUniqueUserId("LoomTest_AddDeck");
            await LoomTestContext.LoomManager.SignUp(user);

            var deck = new Deck
            {
                heroId = 0,
                name = "Gaurav",
                cards = new List<DeckCardData>
                {
                    new DeckCardData
                    {
                        amount = 100500,
                        cardName = "Izze"
                    }
                }
            };

            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.LoomManager.AddDeck(user, deck);
            });
        });
    }

    [UnityTest]
    public IEnumerator EditDeck()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            var user = LoomTestContext.CreateUniqueUserId("LoomTest_EditDeck");
            await LoomTestContext.LoomManager.SignUp(user);

            var deck = new Deck
                { name = "Default" };
            //deck.AddCard(0);
            //deck.AddCard(1);

            await LoomTestContext.LoomManager.EditDeck(user, deck);
        });
    }

    [UnityTest]
    public IEnumerator EditDeck_Wrong_User_Request()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            var user = LoomTestContext.CreateUniqueUserId("LoomTest_EditDeck_wrong_user");
            var deck = new Deck
            {
                heroId = 0,
                name = "Gaurav"
            };
            
            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.LoomManager.EditDeck(user, deck);
            });
        });
    }

    [UnityTest]
    public IEnumerator EditDeck_Wrong_Deck_Request()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            var user = LoomTestContext.CreateUniqueUserId("LoomTest_EditDeck");
            await LoomTestContext.LoomManager.SignUp(user);

            var deck = new Deck
            {
                id = 123,
                heroId = 0,
                name = "GauravRandomDeck"
            };
            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.LoomManager.EditDeck(user, deck);
            });
        });
    }

    [UnityTest]
    public IEnumerator DeleteDeck()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            var user = LoomTestContext.CreateUniqueUserId("LoomTest_DeleteDeck");
            await LoomTestContext.LoomManager.SignUp(user);

            await LoomTestContext.LoomManager.DeleteDeck(user, 0);
        });
    }

    [UnityTest]
    public IEnumerator DeleteDeck_Wrong_User_Request()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            var user = LoomTestContext.CreateUniqueUserId("LoomTest_DeleteDeck_wrong_User");
            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.LoomManager.DeleteDeck(user, 0);
            });
        });
    }

    [UnityTest]
    public IEnumerator DeleteDeck_Wrong_Deck_Request()
    {
        return LoomTestContext.ContractAsyncTest(async () =>
        {
            var user = LoomTestContext.CreateUniqueUserId("LoomTest_DeleteDeck_wrong_Deck");
            await LoomTestContext.LoomManager.SignUp(user);

            await LoomTestContext.AssertThrowsAsync(async () =>
            {
                await LoomTestContext.LoomManager.DeleteDeck(user, 123);
            });
        });
    }
}
