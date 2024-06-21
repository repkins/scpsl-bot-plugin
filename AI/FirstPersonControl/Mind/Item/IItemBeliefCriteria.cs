using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item
{
    internal interface IItemBeliefCriteria : IEquatable<IItemBeliefCriteria>
    {
        bool EvaluateItem(ItemPickupBase item);
        bool EvaluateItem(ItemBase item);
    }
}
