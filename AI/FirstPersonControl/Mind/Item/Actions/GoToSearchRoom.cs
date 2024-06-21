using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions
{
    internal class GoToSearchRoom<C> : GoTo<RoomEnterLocation> where C : IItemBeliefCriteria, IEquatable<C>
    {
        public readonly C Criteria;
        public GoToSearchRoom(C criteria, int idx, FpcBotPlayer botPlayer) : this(idx, botPlayer)
        {
            this.Criteria = criteria;
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ItemSightedLocations<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        private readonly FpcBotPlayer botPlayer;
        public GoToSearchRoom(int idx, FpcBotPlayer botPlayer) : base(idx)
        {
            this.botPlayer = botPlayer;
        }

        public override void Tick()
        {
            var enterPosition = location.Positions[Idx];
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            if (Vector3.Distance(enterPosition, cameraPosition) > 1.25f)
            {
                botPlayer.MoveToPosition(enterPosition);
                return;
            }
        }

        public override void Reset()
        { }

        public override string ToString()
        {
            return $"{nameof(GoToSearchRoom<C>)}({this.Criteria}, {Idx})";
        }
    }
}
