using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Misc;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions
{
    internal class GoToLockerSpawnLocation<C> : IAction where C : IItemBeliefCriteria, IEquatable<C>
    {
        public C Criteria { get; }
        public GoToLockerSpawnLocation(C criteria, FpcBotPlayer botPlayer)
        {
            this.Criteria = criteria;

            this.botPlayer = botPlayer;
        }

        private LockerSpawnLocation lockerSpawnLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this.lockerSpawnLocation = fpcMind.ActionEnabledBy<LockerSpawnLocation>(this, b => true, b => b.Position.HasValue);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ItemInSightedLocker<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        public void Reset()
        {
        }

        public void Tick()
        {
            var spawnPosition = lockerSpawnLocation.Position!.Value;
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            var dist = Vector3.Distance(spawnPosition, cameraPosition);

            if (dist > 1.75f)
            {
                botPlayer.MoveToPosition(spawnPosition);
                return;
            }

            var cameraDirection = botPlayer.BotHub.PlayerHub.PlayerCameraReference.forward;

            if (Vector3.Dot((spawnPosition - cameraPosition).normalized, cameraDirection) <= 1f - .0001f)
            {
                botPlayer.LookToPosition(spawnPosition);
                return;
            }
        }

        public override string ToString()
        {
            return $"{nameof(GoToLockerSpawnLocation<C>)}({this.Criteria})";
        }

        protected readonly FpcBotPlayer botPlayer;
    }
}
