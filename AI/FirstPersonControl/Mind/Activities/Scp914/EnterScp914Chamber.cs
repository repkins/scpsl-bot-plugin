using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class EnterScp914Chamber : IActivity
    {
        private Scp914ChamberDoor _openedDoorSaw;
        private Scp914Chamber _scp914Chamber;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            _openedDoorSaw = fpcMind.ActivityEnabledBy<Scp914ChamberDoor>(this, OfOpened, b => b.Door);
            _scp914Chamber = fpcMind.ActivityEnabledBy<Scp914Chamber>(this, b => !b.IsInside);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<Scp914Chamber>(this);
        }

        public EnterScp914Chamber(FpcBotPlayer botPlayer)
        {
            _botPlayer = botPlayer;
        }

        public void Tick()
        {
            var targetPosition = _openedDoorSaw.Door.transform.position + _scp914Chamber.InsideNormal;

            _botPlayer.MoveToPosition(targetPosition);
        }

        public void Reset()
        {
        }

        private readonly FpcBotPlayer _botPlayer;

        private bool OfOpened(Scp914ChamberDoor obj) => obj.State == DoorState.Opened;
    }
}
