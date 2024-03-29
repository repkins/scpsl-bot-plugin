using Interactables.Interobjects.DoorUtils;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard.Beliefs;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard.Activities
{
    internal class FindKeycard : FindItem<KeycardWithPermissions>
    {
        protected override ItemWithinSight<KeycardWithPermissions> ItemWithinSight => botPlayer.MindRunner.GetBelief<KeycardWithinSight>(OfPermissions);

        public readonly KeycardPermissions Permissions;
        public FindKeycard(KeycardPermissions permissions, FpcBotPlayer botPlayer) : base(botPlayer)
        {
            this.Permissions = permissions;
        }

        private bool OfPermissions(KeycardWithinSight obj) => obj.Criteria.Permissions == Permissions;

        public override string ToString()
        {
            return $"{GetType().Name}({Permissions})";
        }
    }
}
