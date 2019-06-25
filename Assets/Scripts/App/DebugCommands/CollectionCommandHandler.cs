using System;
using System.Collections.Generic;
using System.Linq;
using Loom.Client;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Iap;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public static class CollectionCommandHandler
    {
        private static PlasmaChainBackendFacade _plasmaChainBackendFacade;

        public static void Initialize()
        {
            CommandHandlers.RegisterCommandHandlers(typeof(CollectionCommandHandler));

            _plasmaChainBackendFacade = GameClient.Get<PlasmaChainBackendFacade>();
        }

        [CommandHandler]
        public static async void PlasmaChainGetOwnerCards()
        {
            using (DAppChainClient client = await _plasmaChainBackendFacade.GetConnectedClient())
            {
                IReadOnlyList<CollectionCardData> cardsOwned = await _plasmaChainBackendFacade.GetCardsOwned(client);
                cardsOwned =
                    cardsOwned
                        .OrderBy(c => c.CardKey.MouldId.Id)
                        .ThenBy(c => c.CardKey.Variant)
                        .ToList();

                Debug.Log($"Result: {cardsOwned.Count} cards:\n" + String.Join("\n", cardsOwned));
            }
        }
    }
}
