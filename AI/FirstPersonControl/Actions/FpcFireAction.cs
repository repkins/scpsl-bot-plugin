using InventorySystem.Items.Firearms;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcFireAction : IFpcAction
    {
        public Firearm Firearm { get; set; }

        public FpcFireAction(FpcBotPlayer fpcBotPlayer)
        {
            _botPlayer = fpcBotPlayer;
        }

        public void Reset()
        {
            Firearm = null;
        }

        public void UpdatePlayer()
        {
            if (Firearm.ActionModule.ServerAuthorizeShot())
            {
                Firearm.HitregModule.ClientCalculateHit(out var shotMessage);
                Firearm.HitregModule.ServerProcessShot(shotMessage);
                Firearm.OnWeaponShot();
            }
        }

        private FpcBotPlayer _botPlayer;
    }
}
