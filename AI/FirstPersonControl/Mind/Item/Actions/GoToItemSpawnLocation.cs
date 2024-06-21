using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions
{
    internal class GoToItemSpawnLocation<C> : GoTo<ItemSpawnsLocation<C>, C> where C : IItemBeliefCriteria, IEquatable<C>
    {
        public GoToItemSpawnLocation(C criteria, FpcBotPlayer botPlayer) : base(criteria, 0)
        {
            this.botPlayer = botPlayer;
        }

        public new void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            base.SetEnabledByBeliefs(fpcMind);
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ItemSightedLocations<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        private float closestDist;

        public override void Reset()
        {
            closestDist = float.MaxValue;
        }

        public override void Tick()
        {
            var spawnPosition = location.Positions[Idx];
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            var dist = Vector3.Distance(spawnPosition, cameraPosition);

            closestDist = dist < closestDist ? dist : closestDist;

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
            return $"{nameof(GoToItemSpawnLocation<C>)}({this.Criteria})";
        }

        protected readonly FpcBotPlayer botPlayer;
    }
}
