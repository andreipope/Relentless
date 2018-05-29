using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandDevs.CZB
{
    public interface IGameplayManager
    {
        int PlayerHeroId { get; set; }
        int OpponentHeroId { get; set; }

        T GetController<T>() where T : IController;

        string GetCardSet(Data.Card card);

        void RearrangeHands();
    }    
}
