using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class GoToItemInOutakeChamber<C> : IActivity where C : IItemBeliefCriteria, IEquatable<C>
    {
        public C Criteria { get; }
        public GoToItemInOutakeChamber(C criteria, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            this.Criteria = criteria;
        }

        private ItemInOutakeChamber<C> itemInOutakeChamber;
        private Scp914Location scp914Location;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this.itemInOutakeChamber = fpcMind.ActivityEnabledBy<ItemInOutakeChamber<C>>(this, b => b.PositionRelative.HasValue);
            this.scp914Location = fpcMind.ActivityEnabledBy<Scp914Location>(this, b => b.OutakeChamberPosition.HasValue);

            fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => !b.Is(this.scp914Location.OutakeChamberPosition!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActivityImpacts<ItemSightedLocation<C>>(this, b => b.Criteria.Equals(this.Criteria));
        }

        private readonly FpcBotPlayer botPlayer;
        public GoToItemInOutakeChamber(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void Tick()
        {
            var playerPosition = botPlayer.BotHub.PlayerHub.transform.position;

            var outakeChamberPosition = scp914Location.OutakeChamberPosition!.Value;
            outakeChamberPosition.y = playerPosition.y;

            botPlayer.MoveToPosition(outakeChamberPosition);

            var transformedItemPosition = outakeChamberPosition + itemInOutakeChamber.PositionRelative!.Value;
            botPlayer.LookToPosition(transformedItemPosition);

            if (Vector3.Distance(playerPosition, outakeChamberPosition) <= 0.4f)
            {
                itemInOutakeChamber.Update(null);
            }
        }

        public void Reset()
        {
        }

        public override string ToString()
        {
            return $"{nameof(GoToItemInOutakeChamber<C>)}({this.Criteria})";
        }
    }
}
