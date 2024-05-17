using Interactables;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using System;
using System.Diagnostics;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions
{
    internal class GoToItemInLocker<C> : GoTo<ItemInSightedLocker<C>, C> where C : IItemBeliefCriteria, IEquatable<C>
    {
        public GoToItemInLocker(C criteria, FpcBotPlayer botPlayer) : base(criteria)
        {
            this.botPlayer = botPlayer;
        }

        public new void SetEnabledByBeliefs(FpcMind fpcMind)
        {
            base.SetEnabledByBeliefs(fpcMind);
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ItemSightedLocation<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        public override void Reset()
        {
        }

        public override void Tick()
        {
            var spawnPosition = itemLocation.AccessiblePosition!.Value;
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            var spawnSightPosition = spawnPosition + itemLocation.LockerDirection!.Value * 2;
            spawnSightPosition.y = cameraPosition.y;

            var dist = Vector3.Distance(spawnSightPosition, cameraPosition);

            if (dist > 0.2f)
            {
                botPlayer.MoveToPosition(spawnSightPosition);
                return;
            }

            var lockerOpened = itemLocation.LockerOpened!.Value;
            if (!lockerOpened)
            {
                var lockerDoor = itemLocation.LockerDoor;

                if (!botPlayer.Interact(lockerDoor))
                {
                    var posToChamber = lockerDoor.GetComponent<Collider>().bounds.center;

                    botPlayer.LookToPosition(posToChamber);
                    //Log.Debug($"Looking towards door interactable");
                }
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
            return $"{nameof(GoToItemInLocker<C>)}({this.Criteria})";
        }

        protected readonly FpcBotPlayer botPlayer;
    }
}
