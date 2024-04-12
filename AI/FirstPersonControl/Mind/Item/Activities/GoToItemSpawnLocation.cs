using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using System;
using UnityEngine;

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
            itemSpawnLocation = fpcMind.ActivityEnabledBy<ItemSpawnLocation<C>>(this, b => b.Position.HasValue);
            fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => !b.Is(itemSpawnLocation.Position!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemSightedLocation<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        public GoToItemSpawnLocation(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void Tick()
        {
            var spawnPosition = itemSpawnLocation.Position!.Value;
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            if (Vector3.Distance(spawnPosition, cameraPosition) > 1.75f)
            {
                botPlayer.MoveToPosition(spawnPosition);
                return;
            }
        }

        public void Reset()
        {
        }

        public override string ToString()
        {
            return $"{nameof(GoToItemSpawnLocation<C>)}({this.Criteria})";
        }

        protected readonly FpcBotPlayer botPlayer;
    }
}
