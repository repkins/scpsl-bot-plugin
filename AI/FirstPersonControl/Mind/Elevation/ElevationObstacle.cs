using Interactables.Interobjects;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Elevation
{
    internal class ElevationObstacle : Belief<bool>
    {
        private readonly FpcBotNavigator navigator;
        private readonly SightSense sightSense;

        public ElevationObstacle(FpcBotNavigator botNavigator, SightSense sightSense) 
        {
            this.navigator = botNavigator;
            this.sightSense = sightSense;

            sightSense.OnAfterSightSensing += OnAfterSightSensing;
        }

        private void OnAfterSightSensing()
        {
            var pathOfPoints = navigator.PointsPath;
            if (pathOfPoints.Count == 0)
            {
                return;
            }


            var lastPoint = pathOfPoints.Last();
            var goalPosition = navigator.GoalPosition;

            if (lastPoint == goalPosition)
            {
                Update(null);
                return;
            }

            // path is partial

            if (!sightSense.IsPositionWithinFov(lastPoint))
            {
                return;
            }

            if (sightSense.IsPositionObstructed(lastPoint))
            {
                return;
            }

            if (Physics.Raycast(lastPoint, Vector3.down, out var hit, 2f))
            {
                var elevator = hit.collider.GetComponentInParent<ElevatorChamber>();
                if (elevator)
                {
                    Update(elevator);
                }
            }
        }

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
