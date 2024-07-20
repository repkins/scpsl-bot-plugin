using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Goals
{
    internal class EscapeTheFacility : IGoal
    {
        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            //entranceZoneEnterLocation = fpcMind.GoalEnabledBy<ZoneEnterLocation>(this, b => b.Zone == FacilityZone.Entrance);
            fpcMind.GoalEnabledBy<ZoneWithin, FacilityZone?>(this, b => FacilityZone.Entrance, b => b.Zone);
        }
    }
}
