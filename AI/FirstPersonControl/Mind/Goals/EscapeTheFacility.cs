using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Goals
{
    internal class EscapeTheFacility : IGoal
    {
        private ZoneWithin zoneWithin;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            //entranceZoneEnterLocation = fpcMind.GoalEnabledBy<ZoneEnterLocation>(this, b => b.Zone == FacilityZone.Entrance);
            zoneWithin = fpcMind.GoalEnabledBy<ZoneWithin, FacilityZone?>(this, b => FacilityZone.Entrance, b => b.Zone);
        }
    }
}
