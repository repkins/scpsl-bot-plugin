using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class GoToPickupItem<T> : GoToPickupItem where T : ItemPickupBase
    {
        protected override ItemWithinSight ItemWithinSight => _botPlayer.MindRunner.GetBelief<ItemWithinSight<T>>();
        protected override ItemWithinPickupDistance ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<ItemWithinPickupDistance<T>>();

        public GoToPickupItem(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }

    internal abstract class GoToPickupItem : IActivity
    {
        protected abstract ItemWithinSight ItemWithinSight { get; }
        protected abstract ItemWithinPickupDistance ItemWithinPickupDistance { get; }

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinSight = fpcMind.ActivityEnabledBy(this, ItemWithinSight);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts(this, ItemWithinPickupDistance);
        }

        public bool Condition() => _itemWithinSight.Item;

        public GoToPickupItem(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Tick()
        {
            var targetItemPosition = _itemWithinSight.Item.transform.position;

            _botPlayer.MoveToPosition(targetItemPosition);
        }

        public void Reset() { }

        private ItemWithinSight _itemWithinSight;
        protected readonly FpcBotPlayer _botPlayer;
    }
}
