using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914
{
    internal class ItemInOutakeChamber : IBelief
    {
        public readonly IItemBeliefCriteria Criteria;
        public ItemInOutakeChamber(IItemBeliefCriteria criteria)
        {
            this.Criteria = criteria;
        }

        public Vector3? Position { get; internal set; }

        public event Action OnUpdate;
    }
}
