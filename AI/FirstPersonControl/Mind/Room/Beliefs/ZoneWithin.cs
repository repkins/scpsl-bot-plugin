using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs
{
    internal class ZoneWithin : Belief<FacilityZone?>
    {
        public ZoneWithin(RoomSightSense roomSightSense)
        {
            roomSightSense.OnSensedRoomWithin += OnSensedRoomWithin;
        }

        private void OnSensedRoomWithin(RoomIdentifier room)
        {
            Update(room.Zone);
        }

        public FacilityZone? Zone { get; private set; }

        private void Update(FacilityZone? newZoneValue)
        {
            if (newZoneValue != Zone)
            {
                Zone = newZoneValue;
                InvokeOnUpdate();
            }
        }

        public override string ToString()
        {
            return $"{nameof(ZoneWithin)}: {Zone}";
        }
    }
}
