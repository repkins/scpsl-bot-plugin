using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions
{
    internal class GoToSearchRoom<C> : IAction where C : IItemBeliefCriteria, IEquatable<C>
    {
        public readonly C Criteria;
        public int Idx;
        public GoToSearchRoom(C criteria, int idx, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            this.Criteria = criteria;
            this.Idx = idx;
        }

        private RoomEnterLocation roomEnterLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            roomEnterLocation = fpcMind.ActionEnabledBy<RoomEnterLocation>(this, b => b.Positions.Count > Idx);

            fpcMind.ActionEnabledBy<DoorObstacle>(this, b => !b.Is(roomEnterLocation.Positions[Idx]));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ItemSightedLocation<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        public GoToSearchRoom(FpcBotPlayer botPlayer)
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
            return $"{nameof(GoToSearchRoom<C>)}({this.Criteria}, {Idx})";
        }

        protected readonly FpcBotPlayer botPlayer;
    }
}
