using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Loom.ZombieBattleground
{
    public class SpecificBattlegroundInfo
    {
        public SpecificBattlegroundOverlordInfo PlayerInfo;
        public SpecificBattlegroundOverlordInfo OpponentInfo;

        public SpecificBattlegroundInfo()
        {
            PlayerInfo = new SpecificBattlegroundOverlordInfo();
            OpponentInfo = new SpecificBattlegroundOverlordInfo();
        }

        public class SpecificBattlegroundOverlordInfo
        {
            public int Health;
            public int MaximumHealth;
            public int CurrentGoo;
            public int MaximumGoo;
            public int HeroId;

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

                MaximumHealth = Constants.DefaultPlayerHp;
                Health = MaximumHealth;

                MaximumGoo = Constants.DefaultPlayerGoo;
                CurrentGoo = MaximumGoo;

                HeroId = 0;
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
