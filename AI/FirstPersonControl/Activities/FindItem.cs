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
    internal class FindItem<T> : IActivity where T : ItemBase
    {
        public FindItem(FpcBotPlayer botPlayer, LastKnownItemLocation<T> lastKnownItemLocation)
        {
            _botPlayer = botPlayer;
            _lastKnownItemLocation = lastKnownItemLocation;
        }

        public void Tick()
        {
            var position = _botPlayer.FpcRole.FpcModule.Position;

            var itemPosWithinSight = _botPlayer.Perception.ItemsWithinSight.FirstOrDefault(c => c is T);
            if (itemPosWithinSight)
            {
                _lastKnownItemLocation.Update(itemPosWithinSight.transform.position);
            }
        }

        private readonly FpcBotPlayer _botPlayer;
        private readonly LastKnownItemLocation<T> _lastKnownItemLocation;
    }
}
