using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal interface IItemBeliefCriteria : IEquatable<IItemBeliefCriteria>
    {
        bool EvaluateItem(ItemPickupBase item);
        bool EvaluateItem(ItemBase item);
    }
}
