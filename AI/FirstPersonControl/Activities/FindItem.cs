using InventorySystem.Items;
using PluginAPI.Core.Items;
using SCPSLBot.AI.FirstPersonControl.Beliefs.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Activities
{
    internal class FindItem<I> : IActivity where I : ItemBase
    {
        public void SetImpactsBeliefs(FpcMindRunner fpcMind)
        {
            _lastKnownItemLocation = fpcMind.ActivityImpacts<LastKnownItemLocation<I>>(this);
        }

        public void SetEnabledByBeliefs(FpcMindRunner fpcMind)
        {

        }

        public bool Condition => true;

        public FindItem(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Tick()
        {
            var position = _botPlayer.FpcRole.FpcModule.Position;

            var itemPosWithinSight = _botPlayer.Perception.ItemsWithinSight.FirstOrDefault(c => c is I);
            if (itemPosWithinSight)
            {
                _lastKnownItemLocation.Update(itemPosWithinSight.transform.position);
            }
        }

        private readonly FpcBotPlayer _botPlayer;
        private LastKnownItemLocation<I> _lastKnownItemLocation;
    }
}
