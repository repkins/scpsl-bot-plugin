using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard.Actions
{
    internal class FindKeycard : OldFindItem<KeycardWithPermissions>
    {
        public FindKeycard(KeycardPermissions permissions, FpcBotPlayer botPlayer) : base(new(permissions), botPlayer)
        {
        }
    }
}
