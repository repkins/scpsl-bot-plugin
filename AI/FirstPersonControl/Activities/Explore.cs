using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Usables;
using SCPSLBot.AI.FirstPersonControl.Beliefs.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Activities
{
    [ActivityImpacts<ItemWithinSight<KeycardItem>>()]
    [ActivityImpacts<ItemWithinSight<Medkit>>()]
    [ActivityImpacts<ItemWithinSight<Firearm>>()]
    internal class Explore : IActivity
    {
        public void SetImpactsBeliefs(FpcMindRunner fpcMind)
        {
            fpcMind.ActivityImpacts<ItemWithinSight<KeycardItem>>(this);
        }

        public void SetEnabledByBeliefs(FpcMindRunner fpcMind)
        {

        }

        public bool Condition => true;

        public Explore(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Tick()
        {
            var position = _botPlayer.FpcRole.FpcModule.Position;

            // Set new position leading to unexplored area (room)
            // 1. Select (any) open node within front in adjacent rooms and trace route.
            // 2. Move character to selected node by following traced route.
            // 3. When characted reached selected open node, start from 1.
        }

        private readonly FpcBotPlayer _botPlayer;
    }
}