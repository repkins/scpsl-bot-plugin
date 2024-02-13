using SCPSLBot.Navigation.Mesh;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Room
{
    internal class RoomBase : IBelief
    {
        public Area Area { get; private set; }

        public event Action OnUpdate;

        public void Update(Area area)
        {
            Area = area;
            OnUpdate?.Invoke();
        }
    }
}
