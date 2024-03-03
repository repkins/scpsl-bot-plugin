using Scp914;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914
{
    internal class Scp914RunningOnSetting : IBelief
    {
        public readonly Scp914KnobSetting Setting;
        public Scp914RunningOnSetting(Scp914KnobSetting setting)
        {
            this.Setting = setting;
        }

        public event Action OnUpdate;
    }
}
