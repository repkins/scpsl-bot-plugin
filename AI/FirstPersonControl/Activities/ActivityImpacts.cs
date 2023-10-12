using SCPSLBot.AI.FirstPersonControl.Beliefs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLBot
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class ActivityImpacts<B>: Attribute where B : IBelief
    {

    }
}
