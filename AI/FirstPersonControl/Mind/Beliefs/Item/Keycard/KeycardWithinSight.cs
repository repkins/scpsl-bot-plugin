using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item.Keycard
{
    internal class KeycardWithinSight : KeycardOfPermissions<ItemWithinSightBase>
    {
        public KeycardWithinSight(KeycardPermissions permissions, ItemsWithinSightSense itemsSightSense, ItemWithinSightBase belief) 
            : base(permissions, itemsSightSense, belief)
        { }
    }
}
