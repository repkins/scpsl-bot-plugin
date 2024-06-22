namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal interface IAction
    {
        void SetEnabledByBeliefs(FpcMind fpcMind);
        void SetImpactsBeliefs(FpcMind fpcMind);

        //float Score { get; }
        float Cost { get; }

        void Tick();
        void Reset();
    }
}
