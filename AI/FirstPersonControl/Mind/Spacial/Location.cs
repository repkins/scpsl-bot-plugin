using System;
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
            var changed = false;

            var posCount = Positions.Count;
            var i = 0;
            foreach (var pos in newPositions)
            {
                if (posCount > i)
                {
                    if (Positions[i] != pos)
                    {
                        Positions[i] = pos;
                        changed = true;
                    }
                }
                else
                {
                    Positions.Add(pos);
                    changed = true;
                }

                i++;
            }

            if (posCount > i)
            {
                Positions.RemoveRange(i, posCount - i);
                changed = true;
            }

            if (changed)
            {
                InvokeOnUpdate();
            }
        }

        protected void RemoveAllPositions(Predicate<Vector3> predicate)
        {
            if (Positions.RemoveAll(predicate) > 0)
            {
                InvokeOnUpdate(); 
            }
        }
    }
}
