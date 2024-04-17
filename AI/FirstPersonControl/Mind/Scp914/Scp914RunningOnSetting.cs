using MapGeneration;
using MEC;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class Scp914RunningOnSetting : IBelief
    {
        private readonly RoomSightSense roomSightSense;

        public Scp914RunningOnSetting(RoomSightSense roomSightSense)
        {
            this.roomSightSense = roomSightSense;
            EventManager.RegisterEvents(this);
        }

        [PluginEvent(ServerEventType.Scp914Activate)]
        public void OnActivateEvent(Player _, Scp914KnobSetting setting)
        {
            if (this.roomSightSense.RoomWithin.Name != RoomName.Lcz914)
            {
                return;
            }

            this.Update(setting);

            Timing.RunCoroutine(Scp914RunningCoroutine());
        }

        private IEnumerator<float> Scp914RunningCoroutine()
        {
            yield return Timing.WaitForSeconds(15f);

            this.Update(null);
        }

        public Scp914KnobSetting? RunningKnobSetting;
        public event Action OnUpdate;

        private void Update(Scp914KnobSetting? newSetting)
        {
            if (newSetting != this.RunningKnobSetting)
            {
                this.RunningKnobSetting = newSetting;
                this.OnUpdate?.Invoke();
            }
        }
    }
}
