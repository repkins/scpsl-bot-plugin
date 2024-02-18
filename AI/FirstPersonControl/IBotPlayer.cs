using PlayerRoles;
using PlayerRoles.FirstPersonControl;

namespace SCPSLBot.AI.FirstPersonControl
{
    internal interface IBotPlayer
    {
        void OnRoleChanged();
        void Update();
        void DumpMind();
    }
}
