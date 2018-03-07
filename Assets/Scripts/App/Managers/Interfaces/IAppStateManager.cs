namespace GrandDevs.CZB
{
    public interface IAppStateManager
    {
        Common.Enumerators.AppState AppState { get; set; }
        void ChangeAppState(Common.Enumerators.AppState stateTo);
        void BackAppState();
    }
}