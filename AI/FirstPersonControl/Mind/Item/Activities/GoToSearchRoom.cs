using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal class GoToSearchRoom<C> : IActivity where C : IItemBeliefCriteria, IEquatable<C>
    {
        public readonly C Criteria;
        public GoToSearchRoom(C criteria, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            this.Criteria = criteria;
        }

        private RoomEnterLocation roomEnterLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            roomEnterLocation = fpcMind.ActivityEnabledBy<RoomEnterLocation>(this, b => b.Position.HasValue);
            fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => !b.Is(roomEnterLocation.Position!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemSightedLocation<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        public GoToSearchRoom(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void Tick()
        {
            var enterPosition = roomEnterLocation.Position!.Value;
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            if (Vector3.Distance(enterPosition, cameraPosition) > 1.75f)
            {
                botPlayer.MoveToPosition(enterPosition);
                return;
            }
        }

        public void Reset()
        {
        }

        protected readonly FpcBotPlayer botPlayer;
    }
}
