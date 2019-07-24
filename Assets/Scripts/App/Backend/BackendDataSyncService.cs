using System;
using System.Threading.Tasks;
using log4net;
using Loom.ZombieBattleground.BackendCommunication;
using Loom.ZombieBattleground.Common;
using Loom.ZombieBattleground.Helpers;
using OneOf;
using OneOf.Types;
using Random = UnityEngine.Random;

namespace Loom.ZombieBattleground
{
    public class BackendDataSyncService : IService
    {
        private static readonly ILog Log = Logging.GetLog(nameof(BackendDataSyncService));
        private const string UpdatingCollectionText = "Updating collection...";

        private BackendFacade _backendFacade;
        private IUIManager _uiManager;
        private IDataManager _dataManager;
        private INetworkActionManager _networkActionManager;
        private bool _gotPendingAutoCardCollectionSyncEvent;
        private bool _gotPendingFullCardCollectionSyncEvent;

        private bool GotAnyPendingCardCollectionSyncEvent => _gotPendingAutoCardCollectionSyncEvent || _gotPendingFullCardCollectionSyncEvent;

        public bool IsCollectionDataDirty { get; private set; }

        public void SetCollectionDataDirtyFlag()
        {
            Log.Debug("Collection dirty flag set");
            IsCollectionDataDirty = true;
        }

        public void ResetPendingCardCollectionSyncFlags()
        {
            _gotPendingAutoCardCollectionSyncEvent = false;
            _gotPendingFullCardCollectionSyncEvent = false;
        }

        public async Task<OneOf<Success, Exception>> UpdateCardCollectionWithUi(bool waitForCardCollectionSyncEvent)
        {
            Log.Debug(nameof(UpdateCardCollectionWithUi));
            if (!IsCollectionDataDirty)
            {
                Log.Warn(nameof(UpdateCardCollectionWithUi) + " called when dirty flag is not set, ignoring");
                return new Success();
            }

            _uiManager.DrawPopup<LoadingOverlayPopup>(UpdatingCollectionText);
            if (waitForCardCollectionSyncEvent)
            {
                // Wait a bit, then update collection anyway
                const float waitForCardCollectionSyncEventTimeout = 15;
                bool timedOut =
                    await InternalTools.WaitWithTimeout(waitForCardCollectionSyncEventTimeout, () => GotAnyPendingCardCollectionSyncEvent);
                if (timedOut)
                {
                    Log.Warn("Timed out waiting for auto card collection sync event");
                }
            }

            try
            {
                Log.Debug("Updating card collection");
                await _networkActionManager.ExecuteNetworkTask(async () =>
                    {
                        await _dataManager.LoadCache(Enumerators.CacheDataType.COLLECTION_DATA);
                        await _dataManager.LoadCache(Enumerators.CacheDataType.DECKS_DATA);
                        IsCollectionDataDirty = false;
                    },
                    onUnknownExceptionCallbackFunc: exception =>
                    {
                        Log.Warn("Error while updating card collection:" + exception);
                        return Task.CompletedTask;
                    });

                return new Success();
            }
            catch (Exception e)
            {
                // No additional handling
                return e;
            }
            finally
            {
                _uiManager.HidePopup<LoadingOverlayPopup>();
            }
        }

        private void BackendFacadeOnUserFullCardCollectionSyncEventReceived(BackendFacade.UserFullCardCollectionSyncEventData evt)
        {
            Log.Debug("Got card collection full sync event");
            _gotPendingFullCardCollectionSyncEvent = true;
            SetCollectionDataDirtyFlag();
        }

        private void BackendFacadeOnUserAutoCardCollectionSyncEventReceived(BackendFacade.UserAutoCardCollectionSyncEventData evt)
        {
            Log.Debug("Got card collection auto sync event");
            _gotPendingAutoCardCollectionSyncEvent = true;
            SetCollectionDataDirtyFlag();
        }

        #region IService

        public void Init()
        {
            _dataManager = GameClient.Get<IDataManager>();
            _networkActionManager = GameClient.Get<INetworkActionManager>();
            _uiManager = GameClient.Get<IUIManager>();
            _backendFacade = GameClient.Get<BackendFacade>();

            _backendFacade.UserAutoCardCollectionSyncEventReceived += BackendFacadeOnUserAutoCardCollectionSyncEventReceived;
            _backendFacade.UserFullCardCollectionSyncEventReceived += BackendFacadeOnUserFullCardCollectionSyncEventReceived;
        }

        public void Update() { }

        public void Dispose()
        {
            if (_backendFacade != null)
            {
                _backendFacade.UserAutoCardCollectionSyncEventReceived -= BackendFacadeOnUserAutoCardCollectionSyncEventReceived;
                _backendFacade.UserFullCardCollectionSyncEventReceived -= BackendFacadeOnUserFullCardCollectionSyncEventReceived;
            }
        }

        #endregion
    }
}
