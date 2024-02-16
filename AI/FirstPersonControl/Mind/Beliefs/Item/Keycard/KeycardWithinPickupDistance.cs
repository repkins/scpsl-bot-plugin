using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard
{
    internal class KeycardWithinPickupDistance : KeycardOfPermissions<ItemWithinPickupDistanceBase>
    {
        public KeycardWithinPickupDistance(KeycardPermissions permissions, ItemsWithinSightSense itemsSightSense, ItemWithinPickupDistanceBase belief) 
            : base(permissions, itemsSightSense, belief)
        {
        }
    }
}
