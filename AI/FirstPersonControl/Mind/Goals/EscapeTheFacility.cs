using SCPSLBot.AI.FirstPersonControl.Mind.Escape;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Goals
{
    internal class EscapeTheFacility : IGoal
    {
        private readonly FpcBotPlayer player;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            fpcMind.GoalEnabledBy<PlayerEscaped, bool>(this, b => true, b => false);
        }
    }
}
