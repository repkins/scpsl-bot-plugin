using InventorySystem.Items.Pickups;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemWithinPickupDistance<T> : ItemWithinPickupDistanceBase where T : ItemPickupBase
    {

    }

    internal class ItemWithinPickupDistanceBase : ItemPickup<ItemPickupBase>
    {

    }
}
