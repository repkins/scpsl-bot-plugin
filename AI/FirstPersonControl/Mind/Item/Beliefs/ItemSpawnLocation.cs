using System;
using UnityEngine;
using MapGeneration;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs
{
    internal class ItemSpawnLocation<C> : IBelief where C : IItemBeliefCriteria
    {
        public C Criteria { get; }
        public ItemSpawnLocation(C criteria)
        {
            Criteria = criteria;
        }

        public Vector3? Position { get; private set; }

        public event Action OnUpdate;
    }
}
