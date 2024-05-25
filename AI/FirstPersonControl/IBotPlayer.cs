using System.Collections.Generic;
using Unity.Jobs;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal interface IBotPlayer
    {
        void OnRoleChanged();
        IEnumerator<JobHandle> Update();
        void DumpMind();
    }
}
