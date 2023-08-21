using PlayerRoles.FirstPersonControl;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal interface IFpcBotAction
    {
        void OnEnter();

        void UpdatePlayer(IFpcRole fpcRole);
    }
}
