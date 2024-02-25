using Interactables.Interobjects.DoorUtils;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door
{
    internal class Door<T> : DoorBase<T> where T : DoorVariant
    {
        public Door(DoorState state) : base(state)
        {
        }
    }
}
