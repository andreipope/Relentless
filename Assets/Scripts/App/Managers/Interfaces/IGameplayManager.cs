using GrandDevs.CZB.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandDevs.CZB
{
    public interface IGameplayManager
    {
        event Action OnGameStartedEvent;
        event Action OnGameEndedEvent;
        event Action OnTurnStartedEvent;
        event Action OnTurnEndedEvent;


        int PlayerDeckId { get; set; }
        int OpponentDeckId { get; set; }

        bool GameStarted { get; set; }
        bool IsTutorial { get; set; }

        int TutorialStep { get; set; }

        List<Player> PlayersInGame { get; set; }

        Player WhoseTurn { get; set; }

        T GetController<T>() where T : IController;

        string GetCardSet(Data.Card card);

        void RearrangeHands();


        bool IsLocalPlayerTurn();


        void StartGameplay();
        void StopGameplay();

        void EndGame(Enumerators.EndGameType endGameType);

        Player GetLocalPlayer();
        Player GetOpponentPlayer();   
    }    
}
