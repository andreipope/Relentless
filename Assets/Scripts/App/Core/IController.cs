using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrandDevs.CZB
{
    public interface IController
    {
        void Init();
        void Update();
        void Dispose();
    }
}
