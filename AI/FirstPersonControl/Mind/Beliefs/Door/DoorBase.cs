using Interactables.Interobjects.DoorUtils;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door
{
    internal class DoorBase<T> : IBelief where T : DoorVariant
    {
        public DoorState State;
        public DoorBase(DoorState state)
        {
            this.State = state;
        }

        public T Door;

        public event Action OnUpdate;

        public void Update(T door)
        {
            Door = door;
            OnUpdate?.Invoke();
        }

        public bool Opened => State == DoorState.Opened;
        public bool IsClosed => State == DoorState.Closed;

        public override string ToString()
        {
            return $"{GetType().Name}({State})";
        }
    }

    internal enum DoorState
    {
        Closed = 0,
        Opened = 1,
    }
}
