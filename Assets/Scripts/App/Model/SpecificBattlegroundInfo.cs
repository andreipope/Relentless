using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class SpecificBattlegroundInfoOld
    {
        public SpecificBattlegroundOverlordInfo PlayerInfo;
        public SpecificBattlegroundOverlordInfo OpponentInfo;

        public SpecificBattlegroundInfoOld()
        {
            PlayerInfo = new SpecificBattlegroundOverlordInfo();
            OpponentInfo = new SpecificBattlegroundOverlordInfo();
        }

        public class SpecificBattlegroundOverlordInfo
        {
            public int Defense;
            public int MaximumDefense;
            public int CurrentGoo;
            public int MaximumGoo;
            public int OverlordId;

            public Enumerators.AIType AIType;

            public List<string> CardsInHand;
            public List<string> CardsInDeck;
            public List<UnitOnBoardInfo> CardsOnBoard;
            public List<string> CardsOnGraveyard;

            public string PrimaryOverlordSkill;
            public string SecondaryOverlordSkill;

            public SpecificBattlegroundOverlordInfo()
            {
                CardsInHand = new List<string>();
                CardsInDeck = new List<string>();
                CardsOnBoard = new List<UnitOnBoardInfo>();
                CardsOnGraveyard = new List<string>();

                AIType = Enumerators.AIType.BLITZ_AI;

                MaximumDefense = Constants.DefaultPlayerHp;
                Defense = MaximumDefense;

                MaximumGoo = Constants.DefaultPlayerGoo;
                CurrentGoo = MaximumGoo;

                OverlordId = 0;
            }
        }

        public class UnitOnBoardInfo
        {
            public string Name;
            public bool IsManuallyPlayable;

            public UnitOnBoardInfo()
            {
                Name = string.Empty;
                IsManuallyPlayable = false;
            }
        }
    }
}
