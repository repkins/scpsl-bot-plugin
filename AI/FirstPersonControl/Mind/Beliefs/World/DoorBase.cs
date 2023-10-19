using Interactables.Interobjects.DoorUtils;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World
{
    internal class DoorBase<T> : IBelief where T : DoorVariant
    {
        public T Door;

        public event Action OnUpdate;

        public void Update(T door)
        {
            Door = door;
            OnUpdate?.Invoke();
        }
    }
}
