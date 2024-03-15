using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class StartScp914OnSetting : IActivity
    {
        public readonly Scp914KnobSetting Setting;
        public StartScp914OnSetting(Scp914KnobSetting setting, FpcBotPlayer botPlayer)
        {
            this.Setting = setting;
            this.botPlayer = botPlayer;
        }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityEnabledBy<Scp914Chamber>(this, b => b.IsPlayerAtSide);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<Scp914RunningOnSetting>(this, b => b.Setting == Setting);
        }

        private readonly FpcBotPlayer botPlayer;

        public void Tick()
        {
            byte knobColliderId = 0;
            byte startColliderId = 2;

            var hub = botPlayer.BotHub.PlayerHub;

            // if knob is not on setting then interact with knob until on setting
            // else interact with start knob

            var currentSetting = Scp914Controller.Singleton.KnobSetting;
            if (currentSetting != Setting)
            {
                Scp914Controller.Singleton.ServerInteract(hub, knobColliderId);
            }
            else
            {
                Scp914Controller.Singleton.ServerInteract(hub, startColliderId);
            }
        }

        public void Reset()
        {

        }
    }
}
