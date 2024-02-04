using PlayerRoles.FirstPersonControl;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcPlayAction : IFpcAction
    {
        public FpcPlayAction(FpcBotPlayer fpcBotPlayer)
        {
            _botPlayer = fpcBotPlayer;
            _findPlayerAction = new FpcFindPlayerAction(fpcBotPlayer);
            _followAction = new FollowTarget(fpcBotPlayer);
            _attackAction = new AttackTarget(fpcBotPlayer);
        }

        public void Reset()
        {
            _findPlayerAction.Reset();
            _followAction.Reset();
            _attackAction.Reset();
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

            if (_botPlayer.Perception.PlayersSense.EnemiesWithinSight.Any() && _botPlayer.Perception.InventorySense.HasFirearmInInventory)
            {
                if (!_attackAction.TargetToAttack)
                {
                    var fpcTransform = _botPlayer.FpcRole.FpcModule.transform;
                    var hub = fpcTransform.GetComponentInParent<ReferenceHub>();

                    var closestTarget = _botPlayer.Perception.PlayersSense.EnemiesWithinSight
                        .Select(o => new { hub = o, distSqr = Vector3.SqrMagnitude(o.transform.position - fpcTransform.position) })
                        .Aggregate((a, c) => c.distSqr < a.distSqr ? c : a)
                        .hub;

                    _attackAction.TargetToAttack = closestTarget;
                }

                _attackAction.UpdatePlayer();
            }
        }

        private FpcBotPlayer _botPlayer;

        private FollowTarget _followAction;
        private AttackTarget _attackAction;
        private FpcFindPlayerAction _findPlayerAction;
    }
}
