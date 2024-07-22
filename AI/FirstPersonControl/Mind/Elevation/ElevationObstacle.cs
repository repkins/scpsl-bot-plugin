using Interactables.Interobjects;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using SCPSLBot.Navigation.Mesh;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Elevation
{
    internal class ElevationObstacle : Belief<bool>
    {
        private readonly FpcBotNavigator navigator;
        private readonly SightSense sightSense;

        public ElevationObstacle(SightSense sightSense, FpcBotNavigator botNavigator) 
        {
            this.navigator = botNavigator;
            this.sightSense = sightSense;

            sightSense.OnAfterSightSensing += OnAfterSightSensing;
        }

        private void OnAfterSightSensing()
        {
            var edgelessSegment = navigator.AreaPathSegments.FirstOrDefault(s => !s.Area.ConnectedAreaEdges.ContainsKey(s.NextArea));
            if (edgelessSegment.NextArea == null)
            {
                if (DestinationArea != null && DestinationArea == navigator.GetAreaWithin())
                {
                    Update(null, null, null);
                }

                return;
            }

            // path has edgeless segment

            var lastPoint = edgelessSegment.Area.CenterPosition;

            if (!sightSense.IsPositionWithinFov(lastPoint))
            {
                return;
            }

            if (sightSense.IsPositionObstructed(lastPoint))
            {
                return;
            }

            var goalPosition = navigator.GoalPosition;

            if (Physics.Raycast(lastPoint, Vector3.down, out var hit, 2f))
            {
                var elevator = hit.collider.GetComponentInParent<ElevatorChamber>();
                if (elevator)
                {
                    Update(elevator, goalPosition, edgelessSegment.NextArea);
                }
            }
        }

        public bool Has(Vector3 goalPos) => GoalPosition == goalPos;
        public ElevatorChamber Elevator { get; private set; }
        public Vector3? GoalPosition { get; private set; }
        public Area DestinationArea { get; private set; }

        private void Update(ElevatorChamber newChamberValue, Vector3? goalPos, Area destinationArea)
        {
            if (newChamberValue != Elevator) 
            { 
                Elevator = newChamberValue;
                GoalPosition = goalPos;
                DestinationArea = destinationArea;
                InvokeOnUpdate();
            }
        }

        public override string ToString()
        {
            return $"{nameof(ElevationObstacle)}: {Elevator?.GetType().Name}";
        }
    }
}
