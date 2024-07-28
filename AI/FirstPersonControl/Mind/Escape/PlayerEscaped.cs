using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Escape
{
    internal class PlayerEscaped : Belief<bool>
    {
        public override string ToString()
        {
            return $"{nameof(PlayerEscaped)}";
        }
    }
}
