using Interactables.Interobjects.DoorUtils;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Door
{
    internal class Door<T> : DoorBase<T> where T : DoorVariant
    {
        public Door(DoorState state) : base(state)
        {
        }
    }
}
