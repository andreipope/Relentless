using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandDevs.CZB
{
    public class Player
    {
        public int id;
        public int deckId;

        public int turn;

        public string nickname;

        public int HP { get; set; }
        public int Mana { get; set; }

        public bool IsLocalPlayer { get; set; }

        public List<int> CardsInDeck { get; set; }
        public List<int> CardsInGraveyard { get; set; }
        public List<int> CardsInHand{ get; set; }

        public List<BoardCreature> BoardCards { get; set; }   
    }
}