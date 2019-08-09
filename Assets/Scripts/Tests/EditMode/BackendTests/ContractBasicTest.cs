using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Protobuf;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using CardKey = Loom.ZombieBattleground.Data.CardKey;
using Deck = Loom.ZombieBattleground.Data.Deck;

namespace Loom.ZombieBattleground.Test
{
    [Category("EditQuickSubset")]
    public class ContractBasicTest
    {
        private readonly LoomTestContext LoomTestContext = new LoomTestContext();

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
        public IEnumerator ListDecks()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.BackendFacade.SignUp(user);

                ListDecksResponse listDecksResponse = await LoomTestContext.BackendFacade.ListDecks(user);
                Assert.IsNotNull(listDecksResponse);
                Assert.IsNotNull(listDecksResponse.Decks);

                DecksData decksData = listDecksResponse.FromProtobuf();
                Assert.IsNotNull(decksData);
                Assert.AreEqual(1, decksData.Decks.Count);
                Assert.AreEqual("Default", decksData.Decks[0].Name);
            });
        }

        [UnityTest]
        public IEnumerator ListDecks_NonExistent_User_Request()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                ListDecksResponse listDecksResponse = await LoomTestContext.BackendFacade.ListDecks("GauravIsGreatWorkingInLoom");
                Assert.IsNotNull(listDecksResponse);
                Assert.AreEqual(0, listDecksResponse.Decks.Count);
            });
        }

        [UnityTest]
        public IEnumerator ListDecks_User_Have_No_Deck()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.BackendFacade.SignUp(user);

                ListDecksResponse listDecksResponse = await LoomTestContext.BackendFacade.ListDecks(user);
                Assert.IsNotNull(listDecksResponse);
                Assert.GreaterOrEqual(listDecksResponse.Decks.Count, 1);

                await LoomTestContext.BackendFacade.DeleteDeck(user, new DeckId(1));

                ListDecksResponse newListDecksResponse = await LoomTestContext.BackendFacade.ListDecks(user);
                Assert.IsNotNull(newListDecksResponse);
                Assert.IsNull(newListDecksResponse.Decks.FirstOrDefault(deck => deck.Id == 1));
            });
        }

        [UnityTest]
        public IEnumerator AddDeck()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.BackendFacade.SignUp(user);

                Deck deck = new Deck(new DeckId(0), new OverlordId(0), "Gaurav", null, 0, 0);

                await LoomTestContext.BackendFacade.AddDeck(user, deck);
            });
        }

        [UnityTest]
        public IEnumerator AddDeck_Wrong_User_Request()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                Deck deck = new Deck(new DeckId(0), new OverlordId(0), "Gaurav", null, 0, 0);

                await LoomTestContext.AssertThrowsAsync(
                    async () =>
                    {
                        await LoomTestContext.BackendFacade.AddDeck(user, deck);
                    });
            });
        }

        [UnityTest]
        public IEnumerator AddDeck_Wrong_Deck_Request()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.BackendFacade.SignUp(user);

                List<DeckCardData> cards =
                    new List<DeckCardData>
                    {
                        new DeckCardData(new CardKey(new MouldId(1), Enumerators.CardVariant.Standard), 100500)
                    };
                Deck deck = new Deck(new DeckId(0), new OverlordId(0), "Gaurav", cards, 0, 0);

                await LoomTestContext.AssertThrowsAsync(async () =>
                {
                    await LoomTestContext.BackendFacade.AddDeck(user, deck);
                });
            });
        }

        [UnityTest]
        public IEnumerator EditDeck()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.BackendFacade.SignUp(user);

                Deck deck = new Deck(new DeckId(1), new OverlordId(0), "Default", null, 0, 0);
                await LoomTestContext.BackendFacade.EditDeck(user, deck);
            });
        }

        [UnityTest]
        public IEnumerator EditDeck_Wrong_User_Request()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                Deck deck = new Deck(new DeckId(0), new OverlordId(0), "Gaurav", null, 0, 0);

                await LoomTestContext.AssertThrowsAsync(async () =>
                {
                    await LoomTestContext.BackendFacade.EditDeck(user, deck);
                });
            });
        }

        [UnityTest]
        public IEnumerator EditDeck_Wrong_Deck_Request()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.BackendFacade.SignUp(user);

                Deck deck = new Deck(new DeckId(123), new OverlordId(0), "GauravRandomDeck", null, 0, 0);
                await LoomTestContext.AssertThrowsAsync(async () =>
                {
                    await LoomTestContext.BackendFacade.EditDeck(user, deck);
                });
            });
        }

        [UnityTest]
        public IEnumerator DeleteDeck()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.BackendFacade.SignUp(user);

                await LoomTestContext.BackendFacade.DeleteDeck(user, new DeckId(1));
            });
        }

        [UnityTest]
        public IEnumerator DeleteDeck_Wrong_User_Request()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.AssertThrowsAsync(
                    async () =>
                    {
                        await LoomTestContext.BackendFacade.DeleteDeck(user, new DeckId(0));
                    });
            });
        }

        [UnityTest]
        public IEnumerator DeleteDeck_Wrong_Deck_Request()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.BackendFacade.SignUp(user);

                await LoomTestContext.AssertThrowsAsync(
                    async () =>
                    {
                        await LoomTestContext.BackendFacade.DeleteDeck(user, new DeckId(123));
                    });
            });
        }

        [UnityTest]
        public IEnumerator GetAiDecks()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.BackendFacade.SignUp(user);

                GetAIDecksResponse getAIDecksResponse = await LoomTestContext.BackendFacade.GetAiDecks();
                Assert.IsNotNull(getAIDecksResponse);
                Assert.IsNotNull(getAIDecksResponse.AiDecks);

                List<Data.AIDeck> decksData =
                    getAIDecksResponse.AiDecks
                        .Select(d => d.FromProtobuf())
                        .ToList();
                Assert.GreaterOrEqual(decksData.Count, 1);
            });
        }

        [UnityTest]
        public IEnumerator GetCardCollection()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.BackendFacade.SignUp(user);

                GetCollectionResponse getCollectionResponse = await LoomTestContext.BackendFacade.GetCardCollection(user);
                Assert.IsNotNull(getCollectionResponse);
                Assert.IsNotNull(getCollectionResponse.Cards);
                Assert.GreaterOrEqual(getCollectionResponse.Cards.Count, 0);
            });
        }

        [UnityTest]
        public IEnumerator ListOverlordUserInstances()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.BackendFacade.SignUp(user);

                ListOverlordUserInstancesResponse listOverlordUserInstancesResponse = await LoomTestContext.BackendFacade.ListOverlordUserInstances(user);
                Assert.IsNotNull(listOverlordUserInstancesResponse);
                Assert.IsNotNull(listOverlordUserInstancesResponse.Overlords);

                Assert.DoesNotThrow(() =>
                {
                    listOverlordUserInstancesResponse.Overlords[0].FromProtobuf();
                });

                Debug.Log(JsonUtility.PrettyPrint(listOverlordUserInstancesResponse.Overlords.ToString()));
            });
        }

        [UnityTest]
        public IEnumerator GetOverlordUserInstance()
        {
            return LoomTestContext.ContractAsyncTest(async () =>
            {
                (string user, BigInteger _) = TestHelper.Instance.CreateTestUser();
                await LoomTestContext.BackendFacade.SignUp(user);

                GetOverlordUserInstanceResponse getOverlordUserInstanceResponse =
                    await LoomTestContext.BackendFacade.GetOverlordUserInstance(user, new OverlordId(0));
                Assert.IsNotNull(getOverlordUserInstanceResponse);
                Assert.IsNotNull(getOverlordUserInstanceResponse.Overlord);

                Assert.DoesNotThrow(() =>
                {
                    getOverlordUserInstanceResponse.Overlord.FromProtobuf();
                });
            });
        }
    }
}
