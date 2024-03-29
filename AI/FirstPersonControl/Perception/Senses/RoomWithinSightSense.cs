using MapGeneration;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
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

        public void ProcessSensedItems()
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
        }

        private static void UpdateRoomBelief(RoomBase roomBelief, Area area)
        {
            roomBelief.Update(area);
            Log.Debug($"{roomBelief.GetType().Name} updated: {area}");
        }
    }
}
