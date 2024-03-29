using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard
{
    internal class KeycardWithinPickupDistance : ItemWithinPickupDistance<KeycardWithPermissions>
    {
        public KeycardWithinPickupDistance(KeycardWithPermissions permissions, ItemsWithinSightSense itemsSightSense)
            : base(permissions, itemsSightSense)
        { }
    }
}
