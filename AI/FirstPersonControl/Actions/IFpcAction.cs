using PlayerRoles.FirstPersonControl;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal interface IFpcAction
    {
        void Reset();
        void UpdatePlayer();
    }
}
