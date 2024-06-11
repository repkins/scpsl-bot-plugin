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
using UnityEngine;

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
            yield return Timing.WaitForSeconds(10f);

            this.ItemsTransformedTime = Time.time;

            yield return Timing.WaitForSeconds(5f);

            this.Update(null);
        }

        public Scp914KnobSetting? TargetSetting { get; private set; }
        public bool IsRunningOn(Scp914KnobSetting knobSetting)
        {
            this.TargetSetting = knobSetting;
            return knobSetting == RunningKnobSetting;
        }

        public Scp914KnobSetting? RunningKnobSetting { get; private set; }
        public float? ItemsTransformedTime { get; private set; }

        public event Action OnUpdate;

        private void Update(Scp914KnobSetting? newSetting)
        {
            if (newSetting != this.RunningKnobSetting)
            {
                this.TargetSetting = null;
                this.RunningKnobSetting = newSetting;
                this.OnUpdate?.Invoke();
            }
        }

        public override string ToString()
        {
            return $"{nameof(Scp914RunningOnSetting)}: {this.RunningKnobSetting}";
        }
    }
}
