using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class StartScp914OnSetting : IActivity
    {
        public readonly Scp914KnobSetting Setting;
        public StartScp914OnSetting(Scp914KnobSetting setting)
        {
            this.Setting = setting;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityEnabledBy<Scp914Chamber>(this, b => b.IsInside);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<Scp914RunningOnSetting>(this, b => b.Setting == Setting);
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
