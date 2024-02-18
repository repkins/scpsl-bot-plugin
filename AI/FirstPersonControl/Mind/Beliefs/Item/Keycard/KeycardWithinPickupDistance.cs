using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard
{
    internal class KeycardWithinPickupDistance : ItemWithinPickupDistance<KeycardWithPermissions>
    {
        public KeycardWithinPickupDistance(KeycardWithPermissions permissions, ItemsWithinSightSense itemsSightSense)
            : base(permissions, itemsSightSense)
        { }
    }
}
