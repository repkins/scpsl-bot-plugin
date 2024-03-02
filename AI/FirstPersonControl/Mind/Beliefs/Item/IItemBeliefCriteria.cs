using InventorySystem.Items.Pickups;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal interface IItemBeliefCriteria : IEquatable<IItemBeliefCriteria>
    {
        bool EvaluateItem(ItemPickupBase item);
    }
}
