using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class ItemInIntakeChamber<C> : IBelief where C : IItemBeliefCriteria
    {
        public C Criteria { get; }
        public ItemInIntakeChamber(C criteria)
        {
            this.Criteria = criteria;
        }

        public Vector3? PositionRelative { get; private set; }
        public event Action OnUpdate;

        public void Update(Vector3? newRelativePosition)
        {
            if (newRelativePosition != this.PositionRelative)
            {
                this.PositionRelative = newRelativePosition;
                this.OnUpdate?.Invoke();
            }
        }

        public override string ToString()
        {
            return $"{nameof(ItemInIntakeChamber<C>)}({this.Criteria})";
        }
    }
}
