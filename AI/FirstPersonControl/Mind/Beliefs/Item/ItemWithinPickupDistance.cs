using InventorySystem.Items.Pickups;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemWithinPickupDistance<T> : ItemWithinPickupDistance where T : ItemPickupBase
    {

    }

    internal class ItemWithinPickupDistance : ItemPickup<ItemPickupBase>
    {

    }
}
