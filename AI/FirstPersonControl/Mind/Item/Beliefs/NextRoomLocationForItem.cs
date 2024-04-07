using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class NextRoomLocationForItem<C> : IBelief where C : IItemBeliefCriteria
    {
        internal readonly C Criteria;

        public Vector3? Position { get; internal set; }

        public event Action OnUpdate;
    }
}
