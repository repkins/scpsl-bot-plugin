using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception
{
    internal interface ISense
    {
        void ProcessEnter(Collider other);
        void ProcessExit(Collider other);
        IEnumerator<JobHandle> ProcessSensibility();
        void ProcessSensedItems();
    }
}
