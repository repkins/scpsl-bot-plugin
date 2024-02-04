using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class OpenScp914Room : IActivity
    {
        public bool Condition() => _keycardInInventory.Item.Permissions.HasFlag(KeycardPermissions.ContainmentLevelOne)
                                && _gateWithinSight.Door.RequiredPermissions.RequiredPermissions == KeycardPermissions.ContainmentLevelOne
                                && _gateWithinSight.Door.IsConsideredOpen();

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _keycardInInventory = fpcMind.ActivityEnabledBy<ItemInInventory<KeycardItem>>(this);
            _gateWithinSight = fpcMind.ActivityEnabledBy<DoorWithinSight<PryableDoor>>(this);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            // 914 room opened within sight
            fpcMind.ActivityImpacts<DoorWithinSight<PryableDoor>>(this);
        }

        public void Tick()
        {
            throw new NotImplementedException();
        }

        public void Reset() { }

        private ItemInInventory<KeycardItem> _keycardInInventory;
        private DoorWithinSight<PryableDoor> _gateWithinSight;
    }
}
