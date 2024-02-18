using InventorySystem.Items.Pickups;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal interface IItemBeliefCriteria
    {
        bool EvaluateItem(ItemPickupBase item);
    }
}
