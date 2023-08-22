using InventorySystem.Items;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcEquipAction : IFpcAction
    {
        public ItemBase TargetItem { get; set; }

        public FpcEquipAction(FpcBotPlayer fpcBotPlayer)
        {
            _fpcBotPlayer = fpcBotPlayer;
        }

        public void Reset()
        {
            TargetItem = null;
        }

        public void UpdatePlayer()
        {
            _fpcBotPlayer.BotHub.PlayerHub.inventory.ServerSelectItem(TargetItem.ItemSerial);
        }

        private FpcBotPlayer _fpcBotPlayer;
    }
}
