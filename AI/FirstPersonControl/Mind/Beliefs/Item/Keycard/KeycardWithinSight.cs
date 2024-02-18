using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard
{
    internal class KeycardWithinSight : ItemWithinSight<KeycardWithPermissions>
    {
        public KeycardWithinSight(KeycardWithPermissions permissions, ItemsWithinSightSense itemsSightSense) 
            : base(permissions, itemsSightSense)
        {
        }
    }
}
