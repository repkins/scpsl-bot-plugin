using InventorySystem.Items.Keycards;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Himself;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities
{
    internal class PickupItemKeycardO5 : PickupItem<KeycardPickup, KeycardItem>
    {
        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemInInventoryKeycardO5>(this);
        }

        public override void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _itemWithinPickupDistance = fpcMind.ActivityEnabledBy<ItemWithinPickupDistanceKeycardO5>(this);
        }

        public PickupItemKeycardO5(FpcBotPlayer botPlayer) : base(botPlayer)
        {
        }
    }
}
