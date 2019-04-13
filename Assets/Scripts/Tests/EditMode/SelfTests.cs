using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Test.MultiplayerTests;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Loom.ZombieBattleground.Test
{
    [Category("EditQuickSubset")]
    public class SelfTests
    {
        private static readonly ILog Log = Logging.GetLog(nameof(SelfTests));

        [UnityTest]
        public IEnumerator CheckForMissingCardTests()
        {
            string NormalizeTestName(string name)
            {
                return
                    name
                        .Replace(" ", "")
                        .Replace("-", "");
            }

            List<Type> cardTestFixtureTypes = new List<Type>
            {
                typeof(GeneralMultiplayerTests),
                typeof(WaterCardsTests),
                typeof(AirCardsTests),
                typeof(EarthCardsTests),
                typeof(FireCardsTests),
                typeof(ToxicCardsTests),
                typeof(HiddenCardsTests),
                typeof(LifeCardsTests),
                typeof(ItemsCardsTests)
            };

            List<string> testNames =
                cardTestFixtureTypes
                    .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    .Where(method =>
                        method.GetCustomAttribute<TestAttribute>() != null || method.GetCustomAttribute<UnityTestAttribute>() != null)
                    .Select(method => NormalizeTestName(method.Name))
                    .ToList();

            return TestUtility.AsyncTest(async () =>
            {
                List<Card> cardsWithMissingTests = new List<Card>();
                MultiplayerDebugClient client = new MultiplayerDebugClient();
                int numberOfCardsWithoutAbilities = 0;

                try
                {
                    await client.Start(contract => new DefaultContractCallProxy(contract), new DAppChainClientConfiguration(), enabledLogs: false);

                    foreach (Card card in client.CardLibrary)
                    {
                        // Ignore simple cards, no point in making tests for each of them
                        if (card.Abilities.Count == 0)
                        {
                            numberOfCardsWithoutAbilities++;
                            continue;
                        }

                        if (testNames.Any(testName => testName.IndexOf(NormalizeTestName(card.Name), StringComparison.InvariantCultureIgnoreCase) != -1))
                            continue;

                        cardsWithMissingTests.Add(card);
                    }

                    if (cardsWithMissingTests.Count == 0)
                        return;

                    cardsWithMissingTests =
                        cardsWithMissingTests
                            .OrderBy(card => card.Faction)
                            .ThenBy(card => card.Name.ToLowerInvariant())
                            .ToList();

                    Debug.Log(
                        $"Total {client.CardLibrary.Count} cards in library, " +
                        $"{numberOfCardsWithoutAbilities} cards without any abilities, " +
                        $"{client.CardLibrary.Count - numberOfCardsWithoutAbilities - cardsWithMissingTests.Count} cards with abilities have tests, " +
                        $"{cardsWithMissingTests.Count} cards with missing tests:\n" +
                        String.Join("\n", cardsWithMissingTests)
                        );

                    //Assert.AreEqual(0,cardsWithMissingTests.Count);
                }
                finally
                {
                    await client.Reset();
                }
            });
        }
    }
}
