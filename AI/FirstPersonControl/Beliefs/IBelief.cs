using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Beliefs
{
    internal interface IBelief
    {
        event Action OnUpdate;
    }
}
