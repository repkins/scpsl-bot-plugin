using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class GoToSearchRoomForScp914 : IAction
    {
        public int Idx { get; }
        public GoToSearchRoomForScp914(int idx, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            this.Idx = idx;
        }

        private RoomEnterLocation roomEnterLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionEnabledBy<ZoneWithin, FacilityZone?>(this, b => FacilityZone.LightContainment, b => b.Zone);
            roomEnterLocation = fpcMind.ActionEnabledBy<RoomEnterLocation>(this, b => b.Positions.Count > Idx);

            fpcMind.ActionEnabledBy<DoorObstacle>(this, b => !b.Is(roomEnterLocation.Positions[Idx]));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<Scp914Location>(this);
        }

        private readonly FpcBotPlayer botPlayer;
        private GoToSearchRoomForScp914(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void Tick()
        {
            var enterPosition = roomEnterLocation.Positions[Idx];
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            if (Vector3.Distance(enterPosition, cameraPosition) > 1.25f)
            {
                botPlayer.MoveToPosition(enterPosition);
                return;
            }
        }

        public void Reset()
        {
        }

        public override string ToString()
        {
            return $"{nameof(GoToSearchRoomForScp914)}({Idx})";
        }
    }
}
