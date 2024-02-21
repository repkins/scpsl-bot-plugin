using Interactables.Interobjects.DoorUtils;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door
{
    internal class DoorWithinSight<T> : DoorBase<T> where T : DoorVariant
    {
        public DoorWithinSight(DoorState state) : base(state)
        {
        }
    }
}
