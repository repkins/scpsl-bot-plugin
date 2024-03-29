using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room.Activities
{
    internal abstract class GoToRoom : IActivity
    {
        protected abstract RoomWithinSight RoomWithinSight { get; }
        protected abstract RoomIn RoomIn { get; }

        public bool Condition() => _roomWithinSight.Area != null;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _roomWithinSight = fpcMind.ActivityEnabledBy(this, RoomWithinSight, b => b.Area != null);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts(this, RoomIn);
        }

        public GoToRoom(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Tick()
        {
            var targetItemPosition = _roomWithinSight.Area.CenterPosition;

            _botPlayer.MoveToPosition(targetItemPosition);
        }

        public void Reset() { }

        private RoomWithinSight _roomWithinSight;
        protected readonly FpcBotPlayer _botPlayer;
    }
}
