using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
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
            EventManager.RegisterEvents(this);
        }

        public event Action OnUpdate;

        public bool RunningAtSetting { get; private set; }

        [PluginEvent(ServerEventType.Scp914Activate)]
        public void OnStart(Scp914KnobSetting knobSetting)
        {
            bool newRunningAtSetting;

            if (knobSetting == Setting)
            {
                newRunningAtSetting = true;
            }
            else
            {
                newRunningAtSetting = false;
            }

            if (newRunningAtSetting != RunningAtSetting)
            {
                RunningAtSetting = newRunningAtSetting;
                OnUpdate?.Invoke();
            }

        }
    }
}
