using PluginAPI.Core;
using SCPSLBot.AI.FirstPersonControl.Mind.Door;
using SCPSLBot.AI.FirstPersonControl.Mind.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class GoToDropItemInIntakeChamber<C> : IActivity where C : IItemBeliefCriteria, IEquatable<C>
    {
        public C Criteria { get; }
        public GoToDropItemInIntakeChamber(C criteria, FpcBotPlayer botPlayer) : this(botPlayer)
        {
            this.Criteria = criteria;
        }

        private ItemInInventory<C> itemInInventoryBelief;
        private Scp914Location scp914Location;
        private ItemInIntakeChamber<C> itemInIntakeChamber;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            this.itemInInventoryBelief = fpcMind.ActivityEnabledBy<ItemInInventory<C>>(this, b => b.Criteria.Equals(this.Criteria), b => b.Item);
            this.scp914Location = fpcMind.ActivityEnabledBy<Scp914Location>(this, b => b.IntakeChamberPosition.HasValue);
            fpcMind.ActivityEnabledBy<DoorObstacle>(this, b => !b.Is(scp914Location.IntakeChamberPosition!.Value));
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            this.itemInIntakeChamber = fpcMind.ActivityImpacts<ItemInIntakeChamber<C>>(this, b => b.Criteria.Equals(this.Criteria));
        }

        private readonly FpcBotPlayer botPlayer;
        public GoToDropItemInIntakeChamber(FpcBotPlayer botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public void Tick()
        {
            var playerPosition = botPlayer.BotHub.PlayerHub.transform.position;

            var itemDropPosition = scp914Location.IntakeChamberPosition!.Value;
            playerPosition.y = itemDropPosition.y;


            if (Vector3.Distance(playerPosition, itemDropPosition) > 0.4f)
            {
                Log.Debug($"{Vector3.Distance(playerPosition, itemDropPosition)}");

                botPlayer.MoveToPosition(itemDropPosition);
                return;
            }

            var itemToDrop = itemInInventoryBelief.Item;
            itemToDrop.ServerDropItem();
            itemInIntakeChamber.Update(playerPosition - itemDropPosition);
        }

        public void Reset()
        {
        }

        public override string ToString()
        {
            return $"{nameof(GoToDropItemInIntakeChamber<C>)}({this.Criteria})";
        }
    }
}
