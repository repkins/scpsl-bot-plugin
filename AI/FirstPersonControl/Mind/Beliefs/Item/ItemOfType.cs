using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal class ItemOfType<T> : ItemOfType where T : ItemPickup<ItemPickupBase>, new()
    {
        public ItemOfType(ItemType type, ItemsWithinSightSense itemsSightSense) 
            : base(type, itemsSightSense, new T())
        { }

        public static implicit operator T(ItemOfType<T> ofType) => ofType.belief as T;
    }

    internal abstract class ItemOfType : ItemOfCriteriaBase<ItemPickup<ItemPickupBase>>
    {
        public ItemType ItemType;
        public ItemOfType(ItemType type, ItemsWithinSightSense itemsSightSense, ItemPickup<ItemPickupBase> belief) : base(belief)
        {
            this.ItemType = type;

            _itemsSightSense = itemsSightSense;
            _itemsSightSense.OnSensedItemWithinSight += ProcessSensedItem;
            _itemsSightSense.OnAfterSensedItemsWithinSight += ProcessAbsentItem;
        }

        private readonly ItemsWithinSightSense _itemsSightSense;
        private int numItems = 0;

        private void ProcessSensedItem(ItemPickupBase item)
        {
            if (item.Info.ItemId == ItemType)
            {
                if (!belief.Item)
                {
                    belief.Update(item);
                }
                numItems++;
            }
        }

        private void ProcessAbsentItem()
        {
            if (numItems > 0)
            {
                if (belief.Item)
                {
                    belief.Update(null);
                }
                numItems = 0;
            }
        }
    }
}
