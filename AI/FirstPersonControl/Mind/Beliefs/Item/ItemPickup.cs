using InventorySystem.Items.Pickups;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemPickup<P, C> : IBelief where P : ItemPickupBase where C : struct
    {
        public C Criteria { get; }
        public ItemPickup(C criteria)
        {
            Criteria = criteria;
        }

        public P Item { get; private set; }

        public event Action OnUpdate;

        public void Update(P value)
        {
            Item = value;
            OnUpdate?.Invoke();
        }
    }
}
