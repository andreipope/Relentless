using LoomNetwork.CZB.Common;

namespace LoomNetwork.CZB
{
    public interface IAppStateManager
    {
        Enumerators.AppState AppState { get; set; }

        void ChangeAppState(Enumerators.AppState stateTo);

        void BackAppState();
    }
}
