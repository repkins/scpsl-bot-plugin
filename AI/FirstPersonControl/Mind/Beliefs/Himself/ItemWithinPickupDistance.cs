using InventorySystem.Items.Pickups;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself
{
    internal class ItemWithinPickupDistance<T> : ItemWithinPickupDistance where T : ItemPickupBase
    {

    }

    internal class ItemWithinPickupDistance : ItemPickup<ItemPickupBase>
    {

    }
}
