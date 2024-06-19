using Interactables.Interobjects;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Elevator
{
    internal class ElevatorObstacle : Belief<bool>
    {
        public bool Has() => Elevator;
        public ElevatorChamber Elevator { get; private set; }

        private void Update(ElevatorChamber newChamberValue)
        {
            if (newChamberValue != Elevator) 
            { 
                Elevator = newChamberValue;
                InvokeOnUpdate();
            }
        }
    }
}
