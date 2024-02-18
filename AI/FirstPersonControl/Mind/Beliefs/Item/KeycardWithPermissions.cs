using Interactables.Interobjects.DoorUtils;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item
{
    internal struct KeycardWithPermissions
    {
        public KeycardPermissions Permissions;
        public KeycardWithPermissions(KeycardPermissions permissions)
        {
            this.Permissions = permissions;
        }
    }
}
