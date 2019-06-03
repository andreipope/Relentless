using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Loom.ZombieBattleground.Iap;
using Newtonsoft.Json;
using OneOf;
using OneOf.Types;
using Opencoding.CommandHandlerSystem;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Loom.ZombieBattleground
{
    public static class IapCommandsHandler
    {
        private static IapMediator _iapMediator;
        private static AuthFiatApiFacade _authFiatApiFacade;
        private static FiatPlasmaManager _fiatPlasmaManager;

        public static void Initialize()
        {
            CommandHandlers.RegisterCommandHandlers(typeof(IapCommandsHandler));
            _iapMediator = GameClient.Get<IapMediator>();
            _authFiatApiFacade = GameClient.Get<AuthFiatApiFacade>();
            _fiatPlasmaManager = GameClient.Get<FiatPlasmaManager>();
        }

        [CommandHandler]
        public static async void IapMediatorInitialize()
        {
            OneOf<Success, IapException> result = await _iapMediator.BeginInitialization();
            Debug.Log("Result: " + result);
        }

        [CommandHandler]
        public static void IapMediatorInitiatePurchase(string productId = "booster_pack_1")
        {
            Product product = _iapMediator.Products.Single(p => p.definition.storeSpecificId == productId);
            OneOf<Success, IapPlatformStorePurchaseError> result = _iapMediator.InitiatePurchase(product);
            Debug.Log("Result: " + result);
        }

        [CommandHandler]
        public static async void AuthApiGetTransactions()
        {
            List<AuthFiatApiFacade.TransactionResponse> list = await _authFiatApiFacade.ListPendingTransactions();
            Debug.Log(JsonUtility.PrettyPrint(JsonConvert.SerializeObject(list)));
        }
    }
}
