using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using System;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class GoToItemInOutakeChamber<C> : IAction where C : IItemBeliefCriteria, IEquatable<C>
    {
        public C Criteria { get; }
        public GoToItemInOutakeChamber(C criteria, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            this.Criteria = criteria;
        }

        private ItemInOutakeChamber itemInOutakeChamber;
        private Scp914Location scp914Location;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this.itemInOutakeChamber = fpcMind.ActionEnabledBy<ItemInOutakeChamber>(this, b => b.Criteria.Equals(this.Criteria), b => b.PositionRelative.HasValue);
            this.scp914Location = fpcMind.ActionEnabledBy<Scp914Location>(this, b => b.OutakeChamberPosition.HasValue);

            fpcMind.ActionEnabledBy<DoorObstacle>(this, b => !b.Is(this.scp914Location.OutakeChamberPosition!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ItemSightedLocation<C>>(this, b => b.Criteria.Equals(this.Criteria));
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
