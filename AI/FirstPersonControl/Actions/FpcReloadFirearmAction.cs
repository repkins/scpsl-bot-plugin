using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.BasicMessages;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcReloadFirearmAction : IFpcAction
    {
        public Firearm Firearm { get; set; }

        public FpcReloadFirearmAction(FpcBotPlayer fpcBotPlayer)
        {
            _fpcBotPlayer = fpcBotPlayer;
        }

        public void Reset()
        {
            Firearm = null;
        }

        public void UpdatePlayer()
        {
            var requestMessage = new RequestMessage(Firearm.ItemSerial, RequestType.Reload);
            _fpcBotPlayer.BotHub.ConnectionToServer.Send(requestMessage);
        }
    
        private FpcBotPlayer _fpcBotPlayer;
    }
}
