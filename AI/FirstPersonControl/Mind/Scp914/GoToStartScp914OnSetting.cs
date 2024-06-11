using Interactables;
using PluginAPI.Core;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class GoToStartScp914OnSetting : IAction
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
            scp914Location = fpcMind.ActionEnabledBy<Scp914Location>(this, b => b.ControlsPosition.HasValue);
            scp914Controls = fpcMind.ActionEnabledBy<Scp914Controls>(this, b => true);

            fpcMind.ActionEnabledBy<DoorObstacle>(this, b => !b.Is(scp914Location.ControlsPosition!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpactsWithCondition<Scp914RunningOnSetting>(this, b => b.TargetSetting != KnobSetting);
        }

        private readonly FpcBotPlayer botPlayer;

        public void Tick()
        {
            var playerPosition = this.botPlayer.BotHub.PlayerHub.transform.position;
            var controlsPosition = this.scp914Location.ControlsPosition!.Value;

            if (Vector3.Distance(playerPosition, controlsPosition) > 1f)
            {
                this.botPlayer.MoveToPosition(controlsPosition);
                return;
            }

            var currentSetting = this.scp914Controls.KnobSetting;
            if (currentSetting != this.KnobSetting)
            {
                var settingKnob = this.scp914Controls.SettingKnob;
                if (!settingKnob || !this.botPlayer.Interact(settingKnob))
                {
                    var knobSettingPosition = this.scp914Location.SettingKnobPosition!.Value;
                    this.botPlayer.LookToPosition(knobSettingPosition);
                }
                return;
            }

            var startKnob = this.scp914Controls.StartKnob;
            if (!startKnob || !this.botPlayer.Interact(startKnob))
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
