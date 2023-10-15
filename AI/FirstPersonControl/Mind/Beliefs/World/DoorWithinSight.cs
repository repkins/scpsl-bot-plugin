using Interactables.Interobjects.DoorUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.World
{
    internal class DoorWithinSight<T> : IBelief where T : DoorVariant
    {
        public event Action OnUpdate;

        public T Door;

        public void Update(T door)
        {
            Door = door;
            OnUpdate?.Invoke();
        }
    }
}
