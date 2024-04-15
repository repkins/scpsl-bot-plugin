using Interactables;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class GoToStartScp914OnSetting : IActivity
    {
        public readonly Scp914KnobSetting KnobSetting;
        public GoToStartScp914OnSetting(Scp914KnobSetting knobSetting, FpcBotPlayer botPlayer)
        {
            this.KnobSetting = knobSetting;
            this.botPlayer = botPlayer;
        }

        private Scp914Location scp914Location;
        private Scp914Controls scp914Controls;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            scp914Location = fpcMind.ActivityEnabledBy<Scp914Location>(this, b => b.ControlsPosition.HasValue);
            scp914Controls = fpcMind.ActivityEnabledBy<Scp914Controls>(this, b => true);

            fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => !b.Is(scp914Location.ControlsPosition!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<Scp914RunningOnSetting>(this);
        }

        private readonly FpcBotPlayer botPlayer;

        public void Tick()
        {
            var playerPosition = this.botPlayer.BotHub.PlayerHub.transform.position;
            var controlsPosition = this.scp914Location.ControlsPosition!.Value;

            if (Vector3.Distance(playerPosition, controlsPosition) > 0.01f)
            {
                this.botPlayer.MoveToPosition(controlsPosition);
                return;
            }

            var scp914Controller = Scp914Controller.Singleton;

            var currentSetting = this.scp914Controls.KnobSetting;
            if (currentSetting != this.KnobSetting)
            {
                var settingKnob = this.scp914Controls.SettingKnob;
                if (!settingKnob || !this.botPlayer.Interact(scp914Controller, settingKnob.ColliderId))
                {
                    var knobSettingPosition = this.scp914Location.SettingKnobPosition!.Value;
                    this.botPlayer.LookToPosition(knobSettingPosition);
                }
                return;
            }

            var startKnob = this.scp914Controls.StartKnob;
            if (!startKnob || !this.botPlayer.Interact(scp914Controller, startKnob.ColliderId))
            {
                var startKnobPosition = this.scp914Location.StartKnobPosition!.Value;
                this.botPlayer.LookToPosition(startKnobPosition);
            }
        }

        public void Reset()
        {
        }
    }
}
