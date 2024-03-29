using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard.Activities
{
    internal class GoToPickupKeycard : GoToPickupItem<KeycardWithPermissions>
    {
        public GoToPickupKeycard(KeycardPermissions permissions, FpcBotPlayer botPlayer) : base(new(permissions), botPlayer)
        {
        }
    }
}
