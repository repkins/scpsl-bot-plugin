using SCPSLBot.AI.FirstPersonControl.Beliefs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot.AI.FirstPersonControl.Activities
{
    internal class ActivityEnabledBy<B> : Attribute where B : IBelief
    {

    }
}
