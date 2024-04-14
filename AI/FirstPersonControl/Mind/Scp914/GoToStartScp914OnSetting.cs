using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class GoToStartScp914OnSetting : IActivity
    {
        public readonly Scp914KnobSetting KnobSetting;
        public GoToStartScp914OnSetting(Scp914KnobSetting knobSetting)
        {
            this.KnobSetting = knobSetting;
        }

        private Scp914Location scp914Location;
        private Scp914RunningOnSetting runningOnSetting;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            scp914Location = fpcMind.ActivityEnabledBy<Scp914Location>(this, b => b.ControlsPosition.HasValue);

            fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => !b.Is(scp914Location.ControlsPosition!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            runningOnSetting = fpcMind.ActivityImpacts<Scp914RunningOnSetting>(this);
        }

        public void Tick()
        {
            var currentSetting = Scp914Controller.Singleton.KnobSetting;
        }

        public void Reset()
        {
        }
    }
}
