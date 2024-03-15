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

        public event Action OnUpdate;

        public bool Inside { get; private set; }
        public Vector3? DropPosition { get; private set; }

        public void Update(bool isInside, Vector3? position = null)
        {
            Inside = isInside;
            DropPosition = position;
            OnUpdate?.Invoke();
        }
    }
}
