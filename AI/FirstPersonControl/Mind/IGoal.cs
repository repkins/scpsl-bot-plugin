namespace SCPSLBot.AI.FirstPersonControl.Mind
{
    internal interface IGoal
    {
        void SetEnabledByBeliefs(FpcMind fpcMind);

        bool Condition();
    }
}
