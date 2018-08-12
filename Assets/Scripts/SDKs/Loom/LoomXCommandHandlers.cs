using LoomNetwork.CZB.BackendCommunication;
using Opencoding.CommandHandlerSystem;
using UnityEngine;

namespace LoomNetwork.CZB.BackendCommunication
{
    public static class LoomXCommandHandlers
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            CommandHandlers.RegisterCommandHandlers(typeof(LoomXCommandHandlers));
        }

        [CommandHandler(Description = "Get Contract Write Host Link")]
        private static void GetContractWriteLink()
        {
            CustomDebug.Log("Link =  '" + BackendFacade.Instance.WriterHost);
        }

        [CommandHandler(Description = "Get Contract Reader Host Link")]
        private static void GetContractReaderLink()
        {
            CustomDebug.Log("Link =  '" + BackendFacade.Instance.ReaderHost);
        }

        [CommandHandler(Description = "Init Contract with Write Host Link")]
        private static async void InitContractWithWriteLink(string link)
        {
            BackendFacade.Instance.WriterHost = link;
            await BackendFacade.Instance.CreateContract(BackendFacade.Instance.UserDataModel.PrivateKey);
            CustomDebug.Log("LoomX Initialized..");
        }

        [CommandHandler(Description = "Init Contract with Write Host Link")]
        private static async void InitContractWithReaderLink(string link)
        {
            BackendFacade.Instance.ReaderHost = link;
            await BackendFacade.Instance.CreateContract(BackendFacade.Instance.UserDataModel.PrivateKey);
            CustomDebug.Log("LoomX Initialized..");
        }

        [CommandHandler(Description = "Init Contract with Write and Reader Host Link")]
        private static async void InitContract(string writer, string reader)
        {
            BackendFacade.Instance.WriterHost = writer;
            BackendFacade.Instance.ReaderHost = reader;
            await BackendFacade.Instance.CreateContract(BackendFacade.Instance.UserDataModel.PrivateKey);
            CustomDebug.Log("LoomX Initialized..");
        }
    }
}