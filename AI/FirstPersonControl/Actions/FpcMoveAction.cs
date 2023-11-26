using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Actions
{
    internal class FpcMoveAction : IFpcAction
    {
        public Vector3 TargetPosition;

        public bool IsAtTargetPosition;

        public FpcMoveAction(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Reset()
        {
            IsAtTargetPosition = false;
        }

        public void UpdatePlayer()
        {
            if (Vector3.Distance(TargetPosition, _botPlayer.FpcRole.FpcModule.transform.position) < 1f)
            {
                _botPlayer.Move.DesiredDirection = Vector3.zero;
                IsAtTargetPosition = true;
                return;
            }

            _botPlayer.Move.DesiredDirection = Vector3.Normalize(TargetPosition - _botPlayer.FpcRole.FpcModule.transform.position);
        }

        private FpcBotPlayer _botPlayer;
    }
}
