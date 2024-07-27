using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions
{
    internal class GoToItemSpawnInLocker<C> : GoTo<ItemSpawnsInSightedLocker<C>, C> where C : IItemBeliefCriteria, IEquatable<C>
    {
        public GoToItemSpawnInLocker(C criteria, FpcBotPlayer botPlayer) : base(criteria, 0, botPlayer)
        {
            this.botPlayer = botPlayer;
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ItemSightedLocation<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        public override float Weight { get; } = 1.2f;

        public override void Tick()
        {
            var spawnPosition = location.Positions[Idx];
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            var spawnVisibilityPosition = spawnPosition - location.LockerDirections[spawnPosition];
            spawnVisibilityPosition.y = cameraPosition.y;

            var dist = Vector3.Distance(spawnVisibilityPosition, cameraPosition);

            if (dist > 0.2f)
            {
                botPlayer.MoveToPosition(spawnVisibilityPosition);
                return;
            }

            var lockerDoor = location.LockerDoors[spawnPosition];
            if (lockerDoor)
            {
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

        public override void Reset()
        {
        }

        public override string ToString()
        {
            return $"{nameof(GoToItemSpawnInLocker<C>)}({this.Criteria})";
        }

        protected readonly FpcBotPlayer botPlayer;
    }
}
