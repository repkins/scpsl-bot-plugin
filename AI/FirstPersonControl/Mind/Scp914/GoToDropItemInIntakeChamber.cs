using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class GoToDropItemInIntakeChamber<C> : GoTo<Scp914Location> where C : IItemBeliefCriteria, IEquatable<C>
    {
        public C Criteria { get; }
        public GoToDropItemInIntakeChamber(C criteria, FpcBotPlayer botPlayer) : base(0, botPlayer)
        {
            this.Criteria = criteria;
            this.botPlayer = botPlayer;
        }

        private ItemInInventory<C> itemInInventoryBelief;
        private ItemInIntakeChamber<C> itemInIntakeChamber;

        public override void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this.itemInInventoryBelief = fpcMind.ActionEnabledBy<ItemInInventory<C>>(this, b => b.Criteria.Equals(this.Criteria), b => b.Item);

            base.SetEnabledByBeliefs(fpcMind, () => this.location.IntakeChamberPosition!.Value);
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            this.itemInIntakeChamber = fpcMind.ActionImpacts<ItemInIntakeChamber<C>>(this, b => b.Criteria.Equals(this.Criteria));
        }

        public override float Weight { get; } = 0f;

        private readonly FpcBotPlayer botPlayer;

        public override void Tick()
        {
            var itemDropPosition = location.IntakeChamberPosition!.Value;
            var playerPosition = botPlayer.PlayerPosition with { y = itemDropPosition.y };


            if (Vector3.Distance(playerPosition, itemDropPosition) > 0.4f)
            {
                botPlayer.MoveToPosition(itemDropPosition);
                return;
            }

            var itemToDrop = itemInInventoryBelief.Item;
            itemToDrop.ServerDropItem();
            itemInIntakeChamber.Update(playerPosition - itemDropPosition);
        }

        public override void Reset()
        { }

        public override string ToString()
        {
            return $"{nameof(GoToDropItemInIntakeChamber<C>)}({this.Criteria})";
        }
    }
}
