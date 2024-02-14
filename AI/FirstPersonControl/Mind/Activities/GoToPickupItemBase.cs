using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class GoToPickupItem<T> : GoToPickupItemBase where T : ItemPickupBase
    {
        protected override ItemWithinSightBase ItemWithinSight => _botPlayer.MindRunner.GetBelief<ItemWithinSight<T>>();
        protected override ItemWithinPickupDistanceBase ItemWithinPickupDistance => _botPlayer.MindRunner.GetBelief<ItemWithinPickupDistance<T>>();

        public GoToPickupItem(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }

    internal abstract class GoToPickupItemBase : IActivity
    {
        protected abstract ItemWithinSightBase ItemWithinSight { get; }
        protected abstract ItemWithinPickupDistanceBase ItemWithinPickupDistance { get; }

        public bool Condition() => _itemWithinSight.Item;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinSight = fpcMind.ActivityEnabledBy(this, ItemWithinSight, b => b.Item);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts(this, ItemWithinPickupDistance);
        }

        public GoToPickupItemBase(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Tick()
        {
            var targetItemPosition = _itemWithinSight.Item.transform.position;

            _botPlayer.MoveToPosition(targetItemPosition);
        }

        public void Reset() { }

        private ItemWithinSightBase _itemWithinSight;
        protected readonly FpcBotPlayer _botPlayer;
    }
}
