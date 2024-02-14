using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal interface IActivity
    {
        void SetEnabledByBeliefs(FpcMind fpcMind);
        void SetImpactsBeliefs(FpcMind fpcMind);

        void Tick();
        void Reset();
    }
}
