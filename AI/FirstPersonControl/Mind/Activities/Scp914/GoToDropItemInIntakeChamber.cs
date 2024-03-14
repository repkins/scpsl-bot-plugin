using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Item;
using SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Activities.Scp914
{
    internal class GoToDropItemInIntakeChamber : IActivity
    {
        public readonly ItemType InItemType;
        public GoToDropItemInIntakeChamber(ItemType inItemType, FpcBotPlayer botPlayer)
        {
            this.InItemType = inItemType;
            this.botPlayer = botPlayer;
        }

        private ItemInInventory<ItemOfType> itemToUpgradeInInvetory;
        private ItemInIntakeChamber itemInIntake;

        public void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            itemToUpgradeInInvetory = fpcMind.ActivityEnabledBy<ItemInInventory<ItemOfType>>(this, b => b.Criteria.Equals(InItemType), b => b.Item);
            fpcMind.ActivityEnabledBy<Scp914Chamber>(this, b => b.Inside, b => b.IsPlayerAtSide);
            fpcMind.ActivityEnabledBy<IntakeChamberDoor>(this, b => b.Opened, b => b.Door);
        }

        public void SetImpactsBeliefs(FpcMind fpcMind)
        {
            itemInIntake = fpcMind.ActivityImpacts<ItemInIntakeChamber>(this, b => b.ItemType == InItemType);
        }

        private readonly FpcBotPlayer botPlayer;

        public void Tick()
        {
            RoomIdUtils.TryFindRoom(RoomName.Lcz914, FacilityZone.LightContainment, RoomShape.Endroom, out var scr914Room);

            var dropPosition = scr914Room.transform.TransformPoint(new Vector3(0, 0, 0));

            var playerPos = botPlayer.FpcRole.FpcModule.transform.position;

            if (Vector3.Distance(dropPosition, playerPos) < 0.01f)
            {
                itemToUpgradeInInvetory.Item.ServerDropItem();
            }
            else
            {
                botPlayer.MoveToPosition(dropPosition);
            }
        }

        public void Reset()
        {
        }


    }
}
