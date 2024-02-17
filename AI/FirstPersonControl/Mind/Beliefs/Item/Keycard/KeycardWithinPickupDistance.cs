using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard
{
    internal class KeycardWithinPickupDistance : KeycardWithPermissions<ItemWithinPickupDistanceBase>
    {
        public KeycardWithinPickupDistance(KeycardPermissions permissions, ItemsWithinSightSense itemsSightSense) 
            : base(permissions, itemsSightSense)
        {
        }
    }
}
