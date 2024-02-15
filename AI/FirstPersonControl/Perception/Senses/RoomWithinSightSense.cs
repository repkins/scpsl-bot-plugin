using MapGeneration;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Room;
using SCPSLBot.Navigation.Mesh;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class RoomWithinSightSense : SightSense, ISense
    {
        private readonly FpcBotPlayer _fpcBotPlayer;

        public RoomWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public void ProcessSensibility(Collider collider)
        { }

        public void Reset()
        { }

        public void UpdateBeliefs()
        {
            var playerPosition = _fpcBotPlayer.FpcRole.transform.position;
            var playerForward = _fpcBotPlayer.FpcRole.transform.forward;

            var room = RoomIdUtils.RoomAtPositionRaycasts(playerPosition);
            if (room is null)
            {
                Log.Debug($"Could not determine room bot currently in");
                return;
            }

            var foreignRoomAreasWithinSight = NavigationMesh.Instance.AreasByRoom[room.ApiRoom]
                .Where(a => a.ForeignConnectedAreas.Any())
                .SelectMany(a => a.ForeignConnectedAreas)
                .Select(fa => fa.ConnectedAreas.First())
                .Where(fa => IsWithinFov(playerPosition, playerForward, fa.CenterPosition));

            var scp914WithinSightBelief = _fpcBotPlayer.MindRunner.GetBelief<Scp914RoomWithinSight>(b => true);
            var numScp914rooms = 0;

            foreach (var areaWithinSight in foreignRoomAreasWithinSight)
            {
                if (areaWithinSight.Room.Identifier.Name == RoomName.Lcz914)
                {
                    if (scp914WithinSightBelief.Area is null)
                    {
                        UpdateRoomBelief(scp914WithinSightBelief, areaWithinSight);
                    }
                    numScp914rooms++;
                }
            }
            if (numScp914rooms <= 0 && scp914WithinSightBelief.Area is not null)
            {
                UpdateRoomBelief(scp914WithinSightBelief, null);
            }
        }

        private static void UpdateRoomBelief(RoomBase roomBelief, Area area)
        {
            roomBelief.Update(area);
            Log.Debug($"{roomBelief.GetType().Name} updated: {area}");
        }
    }
}
