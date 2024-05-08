﻿using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using System;
using System.Text;
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
            var stringBuilder = new StringBuilder();
            stringBuilder.Append($"{nameof(ItemInOutakeChamber)}({this.Criteria}): ");

            if (this.PositionRelative.HasValue)
            {
                stringBuilder.Append($"PositionRelative({this.PositionRelative.Value})");
            }

            return stringBuilder.ToString();
        }
    }
}
