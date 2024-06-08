using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Jobs;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight
{
    internal struct ComponentsWithinSightJob<TComponent> : IJob
    {
        [ReadOnly] public GCHandle CollidersWithinSightHandle;
        [ReadOnly] public GCHandle CollidersToComponentHandle;

        [WriteOnly] public GCHandle ComponentsWithinSightHandle;

        public void Execute()
        {
            var withinSight = (List<ColliderData>)CollidersWithinSightHandle.Target;
            var validCollidersToComponent = (Dictionary<ColliderData, TComponent>)CollidersToComponentHandle.Target;

            var componentsWithinSight = (HashSet<TComponent>)ComponentsWithinSightHandle.Target;

            if (componentsWithinSight.Count > 0)
            {
                componentsWithinSight.Clear();
            }
            foreach (var colliderData in withinSight)
            {
                componentsWithinSight.Add(validCollidersToComponent[colliderData]);
            }
        }
    }
}
