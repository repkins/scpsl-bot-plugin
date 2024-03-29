using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard.Activities
{
    internal class FindKeycard : FindItem<KeycardWithPermissions>
    {
        public FindKeycard(KeycardPermissions permissions, FpcBotPlayer botPlayer) : base(new(permissions), botPlayer)
        {
        }
    }
}
