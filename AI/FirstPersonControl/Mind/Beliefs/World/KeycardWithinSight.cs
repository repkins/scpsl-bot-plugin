using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World
{
    internal class KeycardWithinSight<T> : ItemWithinSight<KeycardItem> where T : struct
    {
        //public KeycardWithinSight(KeycardPermissions perms)
        //{
        //    Perms = perms;
        //}

        public KeycardPermissions Perms { get; }
    }
}
