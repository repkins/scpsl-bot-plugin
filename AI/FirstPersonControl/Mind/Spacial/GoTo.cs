using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Misc;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Spacial
{
    internal abstract class GoTo<TLocation> : IAction
        where TLocation : Location
    {
        public int Idx;
        protected GoTo(int idx)
        {
            this.Idx = idx;
        }

        protected virtual TLocation SetEnabledByLocation(FpcMind fpcMind, Func<TLocation, bool> currentGetter)
        {
            return fpcMind.ActionEnabledBy<TLocation>(this, b => b.Positions.Count > Idx);
        }

        protected TLocation location;

        public virtual void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this.location = SetEnabledByLocation(fpcMind, b => b.Positions.Count > Idx);

            fpcMind.ActionEnabledBy<DoorObstacle>(this, b => !b.Is(this.location.Positions[Idx]));
            fpcMind.ActionEnabledBy<GlassObstacle>(this, b => !b.Is(this.location.Positions[Idx]));
        }

        public abstract void SetImpactsBeliefs(FpcMind fpcMind);

        public abstract void Reset();

        public abstract void Tick();
    }
}
