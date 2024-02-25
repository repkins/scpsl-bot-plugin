using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914
{
    internal class Scp914Chamber : IBelief
    {
        public bool IsInside { get; private set; }

        public event Action OnUpdate;

        public void Update(bool isInside)
        {
            IsInside = isInside;
            OnUpdate?.Invoke();
        }
    }
}
