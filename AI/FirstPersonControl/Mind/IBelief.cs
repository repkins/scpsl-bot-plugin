using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal interface IBelief
    {
        event Action OnUpdate;
    }

    internal interface IBelief<C> : IBelief
    {
        C Criteria { get; }
    }
}
