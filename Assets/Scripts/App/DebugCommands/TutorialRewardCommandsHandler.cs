using Loom.ZombieBattleground.Common;

namespace Loom.ZombieBattleground
{
    public static class TutorialRewardCommandsHandler
    {
        private static IGameplayManager _gameplayManager;
    
        public static void Initialize()
        {
            _gameplayManager = GameClient.Get<IGameplayManager>();
        }
        
        public static void SkipToLastTutorial()
        {
            if (!GameClient.Get<ITutorialManager>().IsTutorial)
                return;

            if (GameClient.Get<IAppStateManager>().AppState == Common.Enumerators.AppState.GAMEPLAY)
            {
                GameClient.Get<IGameplayManager>().EndGame(Common.Enumerators.EndGameType.WIN);
                GameClient.Get<IMatchManager>().FinishMatch(Common.Enumerators.AppState.MAIN_MENU);
            }
            else
            {
                GameClient.Get<IAppStateManager>().ChangeAppState(Common.Enumerators.AppState.MAIN_MENU);
            }

            GameClient.Get<ITutorialManager>().StopTutorial(true);
            GameClient.Get<IDataManager>().CachedUserLocalData.CurrentTutorialId = Constants.LastTutorialId;
        }
        
        private static void WinBattle()
        {
            _gameplayManager.OpponentPlayer.Defense = 0;
        }
        
        private static void LoseBattle()
        {
            _gameplayManager.CurrentPlayer.Defense = 0;
        }
    }
}
