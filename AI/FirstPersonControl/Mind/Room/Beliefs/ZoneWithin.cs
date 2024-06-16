using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs
{
    internal class ZoneWithin : IBelief
    {
        public ZoneWithin(RoomSightSense roomSightSense)
        {
            roomSightSense.OnSensedRoomWithin += OnSensedRoomWithin;
        }

        private void OnSensedRoomWithin(RoomIdentifier room)
        {
            Update(room.Zone);
        }

        private FacilityZone? TargetZone;

        public bool Is(FacilityZone targetZone)
        {
            if (targetZone == Zone)
            {
                return true;
            }

            TargetZone = targetZone;
            return false;
        }

        public bool HasTarget(FacilityZone zone)
        {
            if (zone == TargetZone)
            {
                TargetZone = null;
                return true;
            }

            return false;
        }

        private static readonly Dictionary<FacilityZone, int> zones = new()
        {
            { FacilityZone.Surface, 0 },
            { FacilityZone.Entrance, 1 },
            { FacilityZone.HeavyContainment, 2 },
            { FacilityZone.LightContainment, 3 },
        };

        public bool HasTargetWithin(FacilityZone zone, FacilityZone zoneTowardsTarget)
        {
            // zones[zoneTowardsTarget] is between zones[currentZone] and zones[TargetZone]
            if (zone == TargetZone && Zone.HasValue)
            {
                var distToTarget = Math.Abs(zones[Zone.Value] - zones[zone]);
                var distToZoneTowards = Math.Abs(zones[Zone.Value] - zones[zoneTowardsTarget]);

                if (distToZoneTowards < distToTarget)
                {
                    TargetZone = null;
                    return true;
                }
            }

            return false;
        }        

        private FacilityZone? Zone;
        public event Action OnUpdate;

        private void Update(FacilityZone? newZoneValue)
        {
            if (newZoneValue != Zone)
            {
                Zone = newZoneValue;
                OnUpdate?.Invoke();
            }
        }

        public override string ToString()
        {
            return $"{nameof(ZoneWithin)}: {Zone}";
        }
    }
}
