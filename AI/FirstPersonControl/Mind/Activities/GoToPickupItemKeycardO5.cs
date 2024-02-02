using InventorySystem.Items.Keycards;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class GoToPickupItemKeycardO5 : GoToPickupItem<KeycardPickup>
    {
        public override void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinSight = fpcMind.ActivityEnabledBy<ItemWithinSightKeycardO5>(this);
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemWithinPickupDistanceKeycardO5>(this);
        }


        public GoToPickupItemKeycardO5(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }
}
