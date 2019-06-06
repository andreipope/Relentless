using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using log4net;
using Loom.Client;
using Loom.Nethereum.ABI.FunctionEncoding.Attributes;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Data;
using UnityEngine;

namespace Loom.ZombieBattleground.Iap
{
    public class OpenPackPlasmaManager : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(OpenPackPlasmaManager));

        private const int CardsPerPack = 5;

        private const string BalanceOfMethod = "balanceOf";

        private const string ApproveMethod = "approve";

        private const string OpenPackMethod = "openBoosterPack";

        private IDataManager _dataManager;
        private ContractManager _contractManager;

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _contractManager = GameClient.Get<ContractManager>();
        }

        public void Update() { }

        public void Dispose() { }

        public async Task<int> GetPackTypeBalance(DAppChainClient client, Enumerators.MarketplaceCardPackType packTypeId)
        {
            Log.Info($"{nameof(GetPackTypeBalance)}(MarketplaceCardPackType packTypeId = {packTypeId})");

            EvmContract packTypeContract = _contractManager.GetContract(client, GetPackContractTypeFromId(packTypeId));
            int amount = await packTypeContract.StaticCallSimpleTypeOutputAsync<int>(
                BalanceOfMethod,
                Address.FromPublicKey(_contractManager.UserPublicKey).ToString()
            );

            Log.Info($"{nameof(GetPackTypeBalance)} returned {amount}");
            return amount;
        }

        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public async Task<IReadOnlyList<Card>> CallOpenPack(DAppChainClient client, Enumerators.MarketplaceCardPackType packTypeId)
        {
            Log.Info($"{nameof(GetPackTypeBalance)}(MarketplaceCardPackType packTypeId = {packTypeId})");

            EvmContract cardFaucetContract = _contractManager.GetContract(client, IapContractType.CardFaucet);
            EvmContract packContract = _contractManager.GetContract(client, GetPackContractTypeFromId(packTypeId));

            List<Card> cards = new List<Card>();
            void ContractEventReceived(object sender, EvmChainEventArgs e)
            {
                Log.Info($"{nameof(GetPackTypeBalance)}: received smart contract even " + e.EventName);
                GeneratedCardEvent generatedCardEvent = e.DecodeEventDto<GeneratedCardEvent>();
                Log.Info($"{nameof(GetPackTypeBalance)}: CardId = {generatedCardEvent.MouldId}, BoosterType ={generatedCardEvent.BoosterType}</color>");

                if (generatedCardEvent.MouldId % 10 != 0)
                {
                    Log.Warn($"{nameof(GetPackTypeBalance)}: Unknown card with raw MouldId {generatedCardEvent.MouldId}");
                    cards.Add(null);
                    return;
                }

                MouldId mouldId = new MouldId((long) (generatedCardEvent.MouldId / 10));
                (bool found, Card card) = _dataManager.CachedCardsLibraryData.TryGetCardFromMouldId(mouldId);
                if (found)
                {
                    Log.Info($"{nameof(GetPackTypeBalance)}: Matching card {card}");
                    cards.Add(card);
                }
                else
                {
                    Log.Warn($"{nameof(GetPackTypeBalance)}: Unknown card with MouldId {mouldId}");
                    cards.Add(null);
                }
            }

            cardFaucetContract.EventReceived += ContractEventReceived;

            await client.SubscribeToEvents();

            const int amountToApprove = 1;
            await packContract.CallAsync(ApproveMethod, PlasmaChainEndpointsContainer.ContractAddressCardFaucet, amountToApprove);
            await cardFaucetContract.CallAsync(OpenPackMethod, packTypeId);

            const double timeout = 15;
            bool timedOut = false;
            double startTime = Utilites.GetTimestamp();

            await new WaitUntil(() =>
            {
                if (Utilites.GetTimestamp() - startTime > timeout)
                {
                    timedOut = true;
                    return true;
                }

                return cards.Count == CardsPerPack;
            });

            if (timedOut)
                throw new TimeoutException();

            cards = cards.Where(card => card != null).ToList();
            Log.Info($"{nameof(GetPackTypeBalance)} returned {Utilites.FormatCallLogList(cards)}");
            return cards;
        }

        private IapContractType GetPackContractTypeFromId(Enumerators.MarketplaceCardPackType packId)
        {
            switch (packId)
            {
                case Enumerators.MarketplaceCardPackType.Booster:
                    return IapContractType.BoosterPack;
                case Enumerators.MarketplaceCardPackType.Super:
                    return IapContractType.SuperPack;
                case Enumerators.MarketplaceCardPackType.Air:
                    return IapContractType.AirPack;
                case Enumerators.MarketplaceCardPackType.Earth:
                    return IapContractType.EarthPack;
                case Enumerators.MarketplaceCardPackType.Fire:
                    return IapContractType.FirePack;
                case Enumerators.MarketplaceCardPackType.Life:
                    return IapContractType.LifePack;
                case Enumerators.MarketplaceCardPackType.Toxic:
                    return IapContractType.ToxicPack;
                case Enumerators.MarketplaceCardPackType.Water:
                    return IapContractType.WaterPack;
                case Enumerators.MarketplaceCardPackType.Small:
                    return IapContractType.SmallPack;
                case Enumerators.MarketplaceCardPackType.Minion:
                    return IapContractType.MinionPack;
                default:
                    throw new Exception($"Not found ContractType from pack id {packId}");
            }
        }

        [Event("GeneratedCard")]
        private class GeneratedCardEvent
        {
            [Parameter("uint256", "cardId")]
            public BigInteger MouldId { get; set; }

            [Parameter("uint256", "boosterType", 2)]
            public BigInteger BoosterType { get; set; }
        }
    }
}
