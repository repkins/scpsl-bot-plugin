using MapGeneration;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Escape
{
    internal class FacilityEscapeLocation : Location
    {
        public FacilityEscapeLocation(RoomSightSense roomSightSense) 
        {
            roomSightSense.OnSensedRoomWithin += OnSensedRoomWithin;
        }

        private void OnSensedRoomWithin(RoomIdentifier roomWithin)
        {
            if (Positions.Any())
            {
                return;
            }

            if (roomWithin.Name != RoomName.Outside)
            {
                return;
            }

            if (!Physics.Raycast(RoughEscapePosition, Vector3.down, out var hit, 10f, LayerMask.GetMask("Default")))
            {
                Log.Warning($"Raycast down from rough escape position produced no hits.");
                return;
            }

            var escapePosition = hit.point + Vector3.up * 1f;

            AddPosition(escapePosition);
        }

        private static readonly Vector3 RoughEscapePosition = new(124f, 989f, 20f);
    }
}
