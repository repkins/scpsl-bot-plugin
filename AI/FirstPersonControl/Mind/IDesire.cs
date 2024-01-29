namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal interface IDesire
    {
        void SetEnabledByBeliefs(FpcMind fpcMind);

        bool Condition();
    }
}
