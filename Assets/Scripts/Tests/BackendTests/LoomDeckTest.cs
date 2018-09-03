using System.Collections;
using System.Collections.Generic;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine.TestTools;
using Deck = Loom.ZombieBattleground.Data.Deck;

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
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                string user = LoomTestContext.CreateUniqueUserId("LoomTest_GetDeck");
                await LoomTestContext.BackendFacade.SignUp(user);

                ListDecksResponse listDecksResponse = await LoomTestContext.BackendFacade.GetDecks(user);
                Assert.IsNotNull(listDecksResponse);

                DecksData decksData = JsonConvert.DeserializeObject<DecksData>(listDecksResponse.ToString());
                Assert.IsNotNull(decksData);
                Assert.AreEqual(1, decksData.Decks.Count);
                Assert.AreEqual("Default", decksData.Decks[0].Name);
            });
    }

    [UnityTest]
    public IEnumerator GetDeck_Empty_Request()
    {
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                string user = LoomTestContext.CreateUniqueUserId("LoomTest_GetDeck");
                await LoomTestContext.BackendFacade.SignUp(user);
                Assert.IsNull(await LoomTestContext.BackendFacade.GetDecks(string.Empty));
            });
    }

    [UnityTest]
    public IEnumerator GetDeck_Wrong_Request()
    {
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                Assert.IsNull(await LoomTestContext.BackendFacade.GetDecks("GauravIsGreatWorkingInLoom"));
            });
    }

    [UnityTest]
    public IEnumerator GetDeck_User_Have_No_Deck()
    {
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                string user = LoomTestContext.CreateUniqueUserId("LoomTest_NoDecks");
                await LoomTestContext.BackendFacade.SignUp(user);

                ListDecksResponse listDecksResponse = null;
                Assert.IsNull(listDecksResponse);

                listDecksResponse = await LoomTestContext.BackendFacade.GetDecks(user);
                Assert.IsNotNull(listDecksResponse);

                await LoomTestContext.BackendFacade.DeleteDeck(user, 0, 0);

                ListDecksResponse newListDecksResponse = await LoomTestContext.BackendFacade.GetDecks(user);
                Assert.IsNull(newListDecksResponse);
            });
    }

    [UnityTest]
    public IEnumerator AddDeck()
    {
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                string user = LoomTestContext.CreateUniqueUserId("LoomTest_AddDeck");
                await LoomTestContext.BackendFacade.SignUp(user);

                Deck deck = new Deck
                {
                    HeroId = 0,
                    Name = "Gaurav"
                };

                await LoomTestContext.BackendFacade.AddDeck(user, deck, 0);
            });
    }

    [UnityTest]
    public IEnumerator AddDeck_Wrong_User_Request()
    {
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                string user = LoomTestContext.CreateUniqueUserId("LoomTest_AddDeck_wrong_user");
                Deck deck = new Deck
                {
                    HeroId = 0,
                    Name = "Gaurav"
                };

                await LoomTestContext.AssertThrowsAsync(
                    async () =>
                    {
                        await LoomTestContext.BackendFacade.AddDeck(user, deck, 0);
                    });
            });
    }

    [UnityTest]
    public IEnumerator AddDeck_Wrong_Deck_Request()
    {
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                string user = LoomTestContext.CreateUniqueUserId("LoomTest_AddDeck");
                await LoomTestContext.BackendFacade.SignUp(user);

                Deck deck = new Deck
                {
                    HeroId = 0,
                    Name = "Gaurav",
                    Cards = new List<DeckCardData>
                    {
                        new DeckCardData
                        {
                            Amount = 100500,
                            CardName = "Izze"
                        }
                    }
                };

                await LoomTestContext.AssertThrowsAsync(
                    async () =>
                    {
                        await LoomTestContext.BackendFacade.AddDeck(user, deck, 0);
                    });
            });
    }

    [UnityTest]
    public IEnumerator EditDeck()
    {
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                string user = LoomTestContext.CreateUniqueUserId("LoomTest_EditDeck");
                await LoomTestContext.BackendFacade.SignUp(user);

                Deck deck = new Deck
                {
                    Name = "Default"
                };
                await LoomTestContext.BackendFacade.EditDeck(user, deck, 0);
            });
    }

    [UnityTest]
    public IEnumerator EditDeck_Wrong_User_Request()
    {
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                string user = LoomTestContext.CreateUniqueUserId("LoomTest_EditDeck_wrong_user");
                Deck deck = new Deck
                {
                    HeroId = 0,
                    Name = "Gaurav"
                };

                await LoomTestContext.AssertThrowsAsync(
                    async () =>
                    {
                        await LoomTestContext.BackendFacade.EditDeck(user, deck, 0);
                    });
            });
    }

    [UnityTest]
    public IEnumerator EditDeck_Wrong_Deck_Request()
    {
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                string user = LoomTestContext.CreateUniqueUserId("LoomTest_EditDeck");
                await LoomTestContext.BackendFacade.SignUp(user);

                Deck deck = new Deck
                {
                    Id = 123,
                    HeroId = 0,
                    Name = "GauravRandomDeck"
                };
                await LoomTestContext.AssertThrowsAsync(
                    async () =>
                    {
                        await LoomTestContext.BackendFacade.EditDeck(user, deck, 0);
                    });
            });
    }

    [UnityTest]
    public IEnumerator DeleteDeck()
    {
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                string user = LoomTestContext.CreateUniqueUserId("LoomTest_DeleteDeck");
                await LoomTestContext.BackendFacade.SignUp(user);

                await LoomTestContext.BackendFacade.DeleteDeck(user, 0, 0);
            });
    }

    [UnityTest]
    public IEnumerator DeleteDeck_Wrong_User_Request()
    {
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                string user = LoomTestContext.CreateUniqueUserId("LoomTest_DeleteDeck_wrong_User");
                await LoomTestContext.AssertThrowsAsync(
                    async () =>
                    {
                        await LoomTestContext.BackendFacade.DeleteDeck(user, 0, 0);
                    });
            });
    }

    [UnityTest]
    public IEnumerator DeleteDeck_Wrong_Deck_Request()
    {
        return LoomTestContext.ContractAsyncTest(
            async () =>
            {
                string user = LoomTestContext.CreateUniqueUserId("LoomTest_DeleteDeck_wrong_Deck");
                await LoomTestContext.BackendFacade.SignUp(user);

                await LoomTestContext.AssertThrowsAsync(
                    async () =>
                    {
                        await LoomTestContext.BackendFacade.DeleteDeck(user, 123, 0);
                    });
            });
    }
}
