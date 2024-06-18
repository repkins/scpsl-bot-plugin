using Interactables.Interobjects;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Elevator
{
    internal class ElevatorObstacle : IBelief
    {

        public bool Has() => Elevator;

        public ElevatorChamber Elevator { get; private set; }
        public event Action OnUpdate;

        private void Update(ElevatorChamber newChamberValue)
        {
            if (newChamberValue != Elevator) 
            { 
                Elevator = newChamberValue;
                OnUpdate?.Invoke();
            }
        }
    }
}
