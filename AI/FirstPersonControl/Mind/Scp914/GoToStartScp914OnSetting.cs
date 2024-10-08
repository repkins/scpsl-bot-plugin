﻿using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class GoToStartScp914OnSetting : GoTo<Scp914Location>
    {
        public readonly Scp914KnobSetting KnobSetting;
        public GoToStartScp914OnSetting(Scp914KnobSetting knobSetting, FpcBotPlayer botPlayer) : base(0, botPlayer)
        {
            this.KnobSetting = knobSetting;
            this.botPlayer = botPlayer;
        }

        private Scp914Controls scp914Controls;

        public override void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            scp914Controls = fpcMind.GetBelief<Scp914Controls>();

            base.SetEnabledByBeliefs(fpcMind, () => this.location.ControlsPosition!.Value);
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<Scp914RunningOnSetting, Scp914KnobSetting?>(this, b => KnobSetting);
        }

        public override float Weight { get; } = 1.5f;   // "average" effort to target knob setting

        private readonly FpcBotPlayer botPlayer;

        public override void Tick()
        {
            var playerPosition = this.botPlayer.BotHub.PlayerHub.transform.position;
            var controlsPosition = this.location.ControlsPosition!.Value;

            if (Vector3.Distance(playerPosition, controlsPosition) > 1f)
            {
                this.botPlayer.MoveToPosition(controlsPosition);
            }

            var currentSetting = this.scp914Controls.KnobSetting;
            if (currentSetting != this.KnobSetting)
            {
                var settingKnob = this.scp914Controls.SettingKnob;
                if (!settingKnob || !this.botPlayer.Interact(settingKnob))
                {
                    var knobSettingPosition = this.location.SettingKnobPosition!.Value;
                    this.botPlayer.LookToPosition(knobSettingPosition);
                }
                return;
            }

            var startKnob = this.scp914Controls.StartKnob;
            if (!startKnob || !this.botPlayer.Interact(startKnob))
            {
                var startKnobPosition = this.location.StartKnobPosition!.Value;
                this.botPlayer.LookToPosition(startKnobPosition);
            }
        }

        public override void Reset()
        { }

        public override string ToString()
        {
            return $"{nameof(GoToStartScp914OnSetting)}({KnobSetting})";
        }
    }
}
