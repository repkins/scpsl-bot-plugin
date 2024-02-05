using Interactables.Interobjects;
using MapGeneration.Distributors;
using PluginAPI.Core;
using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class LockersWithinSightSense : SightSense, ISense
    {
        public HashSet<Locker> LockersWithinSight { get; } = new();

        public LockersWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;
        }

        public void Reset()
        {
            LockersWithinSight.Clear();
        }

        public void ProcessSensibility(Collider collider)
        {
            if (collider.GetComponentInParent<Locker>() is Locker locker
                && !LockersWithinSight.Contains(locker))
            {
                if (IsWithinSight(collider, locker))
                {
                    LockersWithinSight.Add(locker);
                }
            }
        }

        public void UpdateBeliefs()
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
