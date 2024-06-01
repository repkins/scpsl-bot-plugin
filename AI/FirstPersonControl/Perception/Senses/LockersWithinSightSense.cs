using Interactables;
using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using MapGeneration.Distributors;
using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class LockersWithinSightSense : SightSense<Locker>
    {
        public HashSet<Locker> LockersWithinSight => ComponentsWithinSight;

        public LockersWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        private LayerMask interactableLayerMask = LayerMask.GetMask("InteractableNoPlayerCollision");
        protected override LayerMask layerMask => interactableLayerMask;

        public override void ProcessSightSensedItems()
        {
            //var numLockers = 0u;
            //var lockerWithinSightBelief = _fpcBotPlayer.MindRunner.GetBelief<LockerWithinSight<Locker>>();
            //foreach (var lockerWithinSight in LockersWithinSight)
            //{
            //    if (lockerWithinSight is Locker locker)
            //    {
            //        if (lockerWithinSightBelief.Locker is null)
            //        {
            //            UpdateLockerBelief(lockerWithinSightBelief, locker);
            //        }
            //        numLockers++;
            //    }
            //}
            //if (numLockers <= 0 && lockerWithinSightBelief.Locker is not null)
            //{
            //    UpdateLockerBelief(lockerWithinSightBelief, null as Locker);
            //}
        }

        //private static void UpdateLockerBelief<T, I>(T lockerBelief, I locker) where T : LockerBase<I> where I : Locker
        //{
        //    lockerBelief.Update(locker);
        //    Log.Debug($"{lockerBelief.GetType().Name} updated: {locker}");
        //}

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
