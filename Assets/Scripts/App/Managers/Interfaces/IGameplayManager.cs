using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandDevs.CZB
{
    public interface IGameplayManager
    {
        T GetController<T>() where T : IController;
    }    
}
