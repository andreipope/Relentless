using System.Collections.Generic;
using System.Linq;
using Loom.ZombieBattleground.Common;
using Newtonsoft.Json;

namespace Loom.ZombieBattleground.Data
{
    public class StateData
    {
        public uint MatchId { get; set; }

        public PlayerState CurrentPlayerState { get; set; }

        public PlayerState OpponentPlayerState { get; set; }

        public int Turns { get; private set; }

        [JsonConstructor]
        public StateData(){

        }

        public class PlayerState
        {
            public string Name { get; private set; }

            public uint InitialHp { get; private set; }

            public uint CurrentHp { get; private set; }

            public uint GooVials { get; private set; }

            public uint CurrentGoo { get; private set; }

            public Hero SelfHero { get; }

            public List<BoardUnitView> BoardCards { get; }

            public List<WorkingCard> CardsInDeck { get; }

            public List<WorkingCard> CardsInGraveyard { get; }

            public List<WorkingCard> CardsInHand { get; }

            public List<WorkingCard> CardsOnBoard { get; }

            public AbilityData AbilityData { get; private set; }

        }
    }
}
