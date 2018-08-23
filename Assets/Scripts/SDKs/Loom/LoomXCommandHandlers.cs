#if !UNITY_IOS

using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace LoomNetwork.CZB.BackendCommunication
{
    public static class LoomXCommandHandlers
    {
        private static BackendFacade _backendFacade;
        private static BackendDataControlMediator _backendDataControlMediator;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            CommandHandlers.RegisterCommandHandlers(typeof(LoomXCommandHandlers));
            _backendFacade = new BackendFacade();
            _backendDataControlMediator = new BackendDataControlMediator();
        }

        [CommandHandler(Description = "Get Contract Write Host Link")]
        private static void GetContractWriteLink()
        {
            CustomDebug.Log("Link =  '" + _backendFacade.WriterHost);
        }

        [CommandHandler(Description = "Get Contract Reader Host Link")]
        private static void GetContractReaderLink()
        {
            CustomDebug.Log("Link =  '" + _backendFacade.ReaderHost);
        }

        [CommandHandler(Description = "Init Contract with Write Host Link")]
        private static async void InitContractWithWriteLink(string link)
        {
            _backendFacade.WriterHost = link;
            await _backendFacade.CreateContract(_backendDataControlMediator.UserDataModel.PrivateKey);
            CustomDebug.Log("LoomX Initialized..");
        }

        [CommandHandler(Description = "Init Contract with Write Host Link")]
        private static async void InitContractWithReaderLink(string link)
        {
            _backendFacade.ReaderHost = link;
            await _backendFacade.CreateContract(_backendDataControlMediator.UserDataModel.PrivateKey);
            CustomDebug.Log("LoomX Initialized..");
        }

        [CommandHandler(Description = "Init Contract with Write and Reader Host Link")]
        private static async void InitContract(string writer, string reader)
        {
            _backendFacade.WriterHost = writer;
            _backendFacade.ReaderHost = reader;
            await _backendFacade.CreateContract(_backendDataControlMediator.UserDataModel.PrivateKey);
            CustomDebug.Log("LoomX Initialized..");
        }
    }
}

#endif