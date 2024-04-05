using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Keycard;
using System.Linq;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Door
{
    internal class OpenKeycardDoorObstacle : IActivity
    {
        public readonly KeycardPermissions Permissions;
        public OpenKeycardDoorObstacle(KeycardPermissions permissions)
        {
            this.Permissions = permissions;
        }

        private DoorObstacle doorObstacleBelief;
        private ItemInInventory<KeycardWithPermissions> keycardInInventory;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            doorObstacleBelief = fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => b.GetLastDoor(Permissions));
            keycardInInventory = fpcMind.ActivityEnabledBy<ItemInInventory<KeycardWithPermissions>>(this, b => b.Criteria.Equals(new (Permissions)), b => b.Item);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<DoorObstacle>(this);
        }

        public void Tick()
        {
            var keycard = keycardInInventory.Item;
            if (!keycard.IsEquipped)
            {
                keycard.Owner.inventory.ServerSelectItem(keycard.ItemSerial);
            }

            var doorToOpen = doorObstacleBelief.GetLastDoor(Permissions);

            // TODO: door interaction logic
        }

        public void Reset()
        {

        }
    }
}
