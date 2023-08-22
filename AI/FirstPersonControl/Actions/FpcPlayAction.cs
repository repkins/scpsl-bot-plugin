using PlayerRoles.FirstPersonControl;
using System.Linq;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcPlayAction : IFpcAction
    {
        public FpcPlayAction(FpcBotPlayer fpcBotPlayer)
        {
            _botPlayer = fpcBotPlayer;
            _findPlayerAction = new FpcFindPlayerAction(fpcBotPlayer);
            _followAction = new FpcFollowAction(fpcBotPlayer);
            _shootAction = new FpcShootAction(fpcBotPlayer);
        }

        public void Reset()
        {
            _findPlayerAction.Reset();
            _followAction.Reset();
            _shootAction.Reset();
        }

        public void UpdatePlayer()
        {            
            if (!_followAction.TargetToFollow || _followAction.IsTargetLost)
            {
                _findPlayerAction.UpdatePlayer();

                if (_findPlayerAction.FoundPlayer)
                {
                    _followAction.TargetToFollow = _findPlayerAction.FoundPlayer;
                }
            }

            if (_followAction.TargetToFollow)
            {
                _followAction.UpdatePlayer();
            }

            if (_botPlayer.Perception.EnemiesWithinSight.Any() && _botPlayer.Perception.HasFirearmInInventory)
            {
                _shootAction.UpdatePlayer();
            }
        }

        private FpcBotPlayer _botPlayer;

        private FpcFollowAction _followAction;
        private FpcShootAction _shootAction;
        private FpcFindPlayerAction _findPlayerAction;
    }
}
