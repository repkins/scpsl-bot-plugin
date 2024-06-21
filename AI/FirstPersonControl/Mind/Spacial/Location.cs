using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Spacial
{
    internal class Location : Belief<bool>
    {
        public readonly List<Vector3> Positions = new();

        protected void AddPosition(Vector3 position)
        {
            if (!Positions.Contains(position))
            {
                Positions.Add(position);
                InvokeOnUpdate();
            }
        }

        protected void RemovePosition(Vector3 position)
        {
            if (Positions.Remove(position))
            {
                InvokeOnUpdate();
            }
        }

        protected void SetPositions(IEnumerable<Vector3> newPositions)
        {
            Positions.Clear();
            Positions.AddRange(newPositions);

            InvokeOnUpdate();
        }
    }
}
