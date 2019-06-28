using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Loom.Client;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Data;
using Loom.ZombieBattleground.Iap;
using Loom.ZombieBattleground.Protobuf;
using Newtonsoft.Json;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace Loom.ZombieBattleground
{
    public static class RewardsCommandsHandler
    {
        private static IapMediator _iapMediator;
        private static AuthFiatApiFacade _authFiatApiFacade;
        private static PlasmachainBackendFacade _plasmaChainBackendFacade;
        private static BackendFacade _backendFacade;
        private static BackendDataControlMediator _backendDataControlMediator;

        public static void Initialize()
        {
            CommandHandlers.RegisterCommandHandlers(typeof(RewardsCommandsHandler));
            _iapMediator = GameClient.Get<IapMediator>();
            _authFiatApiFacade = GameClient.Get<AuthFiatApiFacade>();
            _plasmaChainBackendFacade = GameClient.Get<PlasmachainBackendFacade>();
            _backendFacade = GameClient.Get<BackendFacade>();
            _backendDataControlMediator = GameClient.Get<BackendDataControlMediator>();
        }

        [CommandHandler]
        public static async void GetPendingMintingTransactionReceipts(string userId = null)
        {
            if (userId == null)
            {
                userId = _backendDataControlMediator.UserDataModel.UserId;
            }

            GetPendingMintingTransactionReceiptsResponse response = await _backendFacade.GetPendingMintingTransactionReceipts(userId);
            List<AuthFiatApiFacade.TransactionReceipt> receipts = response.ReceiptCollection.Receipts.Select(receipt => receipt.FromProtobuf()).ToList();
            Debug.Log("Result: \n" + JsonConvert.SerializeObject(receipts, Formatting.Indented));

            string txIds = "Tx Ids:\n";
            foreach (MintingTransactionReceipt receipt in response.ReceiptCollection.Receipts)
            {
                txIds += receipt.TxId.FromProtobuf() + "\n";
            }

            Debug.Log(txIds);
        }

        [CommandHandler]
        public static async void ConfirmPendingMintingTransactionReceipt(string txId, string userId = null)
        {
            if (userId == null)
            {
                userId = _backendDataControlMediator.UserDataModel.UserId;
            }

            if (!BigInteger.TryParse(txId, out BigInteger txIdInt))
            {
                Debug.LogError("failed to parse txId");
                return;
            }

            await _backendFacade.ConfirmPendingMintingTransactionReceipt(userId, txIdInt);
            Debug.Log("Done!");
        }

        [CommandHandler(Description = "Give user a booster. If userId is left as -1, current user id will be used.")]
        public static async void DebugMintBoosterPackReceipt(int boosterAmount, int userId = -1)
        {
            if (userId == -1)
            {
                userId = (int) _backendDataControlMediator.UserDataModel.UserIdNumber;
            }

            DebugMintBoosterPackReceiptResponse response = await _backendFacade.DebugMintBoosterPackReceipt(userId, boosterAmount);
            Debug.Log("Result: \n" + JsonUtility.PrettyPrint(response.ReceiptJson));
        }

        [CommandHandler(Description = "Mint boosters and claim them immediately. If userId is left as -1, current user id will be used.")]
        public static async void DebugMintAndClaimBoosterPacks(int boosterPackAmount, int userId = -1)
        {
            if (userId == -1)
            {
                userId = (int) _backendDataControlMediator.UserDataModel.UserIdNumber;
            }

            try
            {
                DebugMintBoosterPackReceiptResponse response = await _backendFacade.DebugMintBoosterPackReceipt(userId, boosterPackAmount);
                using (DAppChainClient client = await _plasmaChainBackendFacade.GetConnectedClient())
                {
                    await _plasmaChainBackendFacade.ClaimPacks(client, response.Receipt.FromProtobuf());
                }

                Debug.Log($"Added {boosterPackAmount} packs!");
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
