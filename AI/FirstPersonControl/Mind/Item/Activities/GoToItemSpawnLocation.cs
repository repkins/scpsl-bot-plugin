using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using UnityEngine;
using System;
using SCPSLBot.AI.FirstPersonControl.Mind.Misc;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Activities
{
    internal class GoToItemSpawnLocation<C> : IActivity where C : IItemBeliefCriteria, IEquatable<C>
    {
        public readonly C Criteria;
        public GoToItemSpawnLocation(C criteria, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            this.Criteria = criteria;
        }

        private ItemSpawnLocation<C> itemSpawnLocation;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this.itemSpawnLocation = fpcMind.ActivityEnabledBy<ItemSpawnLocation<C>>(this, b => b.Criteria.Equals(this.Criteria), b => b.Position.HasValue);

            fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => !b.Is(this.itemSpawnLocation.Position!.Value));
            fpcMind.ActivityEnabledBy<GlassObstacle>(this, b => !b.Is(this.itemSpawnLocation.Position!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemSightedLocation<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        public GoToItemSpawnLocation(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        private float closestDist;

        public void Reset()
        {
            closestDist = float.MaxValue;
        }

        public void Tick()
        {
            var spawnPosition = itemSpawnLocation.Position!.Value;
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
