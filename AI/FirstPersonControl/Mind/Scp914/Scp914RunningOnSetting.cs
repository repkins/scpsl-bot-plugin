using Scp914;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class Scp914RunningOnSetting : IBelief
    {
        public readonly Scp914KnobSetting KnobSetting;
        public Scp914RunningOnSetting(Scp914KnobSetting knobSetting)
        {
            this.KnobSetting = knobSetting;
        }


        public event Action OnUpdate;
    }
}
