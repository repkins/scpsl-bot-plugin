using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Activities
{
    internal interface IActivity
    {
        bool Condition { get; }

        void SetEnabledByBeliefs(FpcMindRunner fpcMind);
        void SetImpactsBeliefs(FpcMindRunner fpcMind);
        void Tick();
    }
}
