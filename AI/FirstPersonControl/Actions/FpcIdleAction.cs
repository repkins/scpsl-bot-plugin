namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcIdleAction : IFpcAction
    {
        public FpcIdleAction(FpcBotPlayer fpcBotPlayer)
        {
            _fpcBotPlayer = fpcBotPlayer;
        }

        public void Reset()
        { }

        public void UpdatePlayer()
        { }
    
        private FpcBotPlayer _fpcBotPlayer;
    }
}
