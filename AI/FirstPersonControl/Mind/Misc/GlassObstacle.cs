using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Misc
{
    internal class GlassObstacle : Belief<bool>
    {
        private readonly FpcBotNavigator navigator;

        public GlassObstacle(GlassSightSense glassSight, FpcBotNavigator navigator)
        {
            this.navigator = navigator;
            glassSight.OnSensedWindowsWithinSight += OnSensedWindowsWithinSight;
        }

        private readonly Queue<Vector3> removeQueue = new();

        private void OnSensedWindowsWithinSight(BreakableWindow window)
        {
            var windowColliders = window.GetComponentsInChildren<Collider>();

            // Remove windows not obstructing paths anymore

            var goalPositions = Windows.Where(p => p.Value == window).Select(p => p.Key);
            foreach (var goalPos in goalPositions)
            {
                var ray = Rays[goalPos];
                if (!windowColliders.Any(collider => collider.Raycast(ray, out _, 1f)))
                {
                    removeQueue.Enqueue(goalPos);
                }
            }

            while (removeQueue.Count > 0)
            {
                Remove(removeQueue.Dequeue());
            }

            // Add window if obstructs current navigation path

            var pathOfPoints = navigator.PointsPath;
            var rays = pathOfPoints.Zip(pathOfPoints.Skip(1), (point, nextPoint) => new Ray(point, nextPoint - point));

            var hitRay = rays.Where(ray => windowColliders.Any(collider => collider.Raycast(ray, out _, 1f)))
                .Select(r => new Ray?(r))
                .FirstOrDefault();

            if (hitRay.HasValue)
            {
                var newGoalPos = pathOfPoints.Last();
                Add(window, newGoalPos, hitRay.Value);
            }
        }

        private void Add(BreakableWindow window, Vector3 goalPos, Ray ray)
        {
            if (!Windows.ContainsKey(goalPos) || Windows[goalPos] != window)
            {
                Windows[goalPos] = window;
                Rays[goalPos] = ray;
                InvokeOnUpdate();
            }
        }

        private void Remove(Vector3 goalPos)
        {
            if (Windows.ContainsKey(goalPos))
            {
                Windows.Remove(goalPos);
                Rays.Remove(goalPos);
                InvokeOnUpdate();
            }
        }

        public bool IsAny => Windows.Count > 0;
        public Dictionary<Vector3, BreakableWindow> Windows { get; } = new();
        public Dictionary<Vector3, Ray> Rays { get; } = new();

        public bool Is(Vector3 goalPos)
        {
            return Windows.ContainsKey(goalPos);
        }

        public override string ToString()
        {
            return $"{nameof(GlassObstacle)}: {string.Join(", ", this.Windows.Values.Select(d => $"{d.transform.position}"))}";
        }
    }
}
