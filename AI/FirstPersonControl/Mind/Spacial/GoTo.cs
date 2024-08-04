using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Elevation;
using SCPSLBot.AI.FirstPersonControl.Mind.Misc;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Spacial
{
    internal abstract class GoTo<TLocation> : IAction
        where TLocation : Location
    {
        public int Idx;
        protected GoTo(int idx, FpcBotPlayer botPlayer)
        {
            this.Idx = idx;
            this.botPlayer = botPlayer;
        }

        protected virtual TLocation SetEnabledByLocation(FpcMind fpcMind, Func<TLocation, bool> currentGetter)
        {
            return fpcMind.ActionEnabledBy<TLocation>(this, currentGetter);
        }

        protected TLocation location;

        public virtual void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            SetEnabledByBeliefs(fpcMind, () => this.location.Positions[Idx]);
        }

        protected virtual void SetEnabledByBeliefs(FpcMind fpcMind, Func<Vector3> targetPositionGetter)
        {
            this.location = SetEnabledByLocation(fpcMind, b => b.Positions.Count > Idx);

            fpcMind.ActionEnabledBy<DoorObstacle, DoorEntry?>(this, b => b.GetEntry(targetPositionGetter()), c => !c.HasValue);
            fpcMind.ActionEnabledBy<GlassObstacle>(this, b => !b.Is(targetPositionGetter()));
            fpcMind.ActionEnabledBy<ElevationObstacle>(this, b => !b.Has(targetPositionGetter()));
        }

        private readonly FpcBotPlayer botPlayer;

        public abstract void SetImpactsBeliefs(FpcMind fpcMind);

        private const float DefaultDistance = 10f;

        private float Distance => location.Positions.Count > Idx ? 
            Vector3.Distance(location.Positions[Idx], botPlayer.CameraPosition) :
            DefaultDistance;

        public abstract float Weight { get; }
        public virtual float Cost => Distance * Weight;

        public abstract void Reset();

        public abstract void Tick();
    }
}
