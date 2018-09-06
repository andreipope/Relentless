using Loom.ZombieBattleground.Common;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class SpecificBattlegroundInfo
    {
        public int CurrentTurn;

        public SpecificBattlegroundOverlordInfo PlayerInfo;
        public SpecificBattlegroundOverlordInfo OpponentInfo;

        public SpecificBattlegroundInfo()
        {
            CurrentTurn = Constants.FirstGameTurnIndex;
            PlayerInfo = new SpecificBattlegroundOverlordInfo();
            OpponentInfo = new SpecificBattlegroundOverlordInfo();
        }

        public class SpecificBattlegroundOverlordInfo
        {
            public int Health;
            public int MaximumHealth;
            public int CurrentGoo;
            public int MaximumGoo;

            public List<string> CardsInHand;
            public List<string> CardsInDeck;
            public List<string> CardsOnBoard;
            public List<string> CardsOnGraveyard;

            public SpecificBattlegroundOverlordInfo()
            {
                CardsInHand = new List<string>();
                CardsInDeck = new List<string>();
                CardsOnBoard = new List<string>();
                CardsOnGraveyard = new List<string>();

                MaximumHealth = Constants.DefaultPlayerHp;
                Health = MaximumHealth;

                MaximumGoo = Constants.DefaultPlayerGoo;
                Health = MaximumGoo;
            }
        }
    }
}
