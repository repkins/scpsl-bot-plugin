using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception
{
    internal interface ISense
    {
        void ProcessSensibility(Collider collider);
        void UpdateBeliefs();
    }
}
