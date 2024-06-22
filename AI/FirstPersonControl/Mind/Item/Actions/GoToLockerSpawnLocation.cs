using MapGeneration.Distributors;
using SCPSLBot.AI.FirstPersonControl.Mind.Item.Beliefs;
using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Item.Actions
{
    internal class GoToLockerSpawnLocation<C> : GoTo<LockerSpawnsLocation> where C : IItemBeliefCriteria, IEquatable<C>
    {
        public C Criteria { get; }
        public StructureType StructureType { get; }
        public GoToLockerSpawnLocation(C criteria, StructureType structureType, FpcBotPlayer botPlayer) : base(0, botPlayer)
        {
            this.Criteria = criteria;
            this.StructureType = structureType;

            this.botPlayer = botPlayer;
        }

        public override void SetImpactsBeliefs(FpcMind fpcMind)
        {
            fpcMind.ActionImpacts<ItemSpawnsInSightedLocker<C>>(this, b => b.Criteria.Equals(Criteria));
        }

        public override float Weight { get; } = 1f / ((1f / 4f) * 3f - Mathf.Pow(1f / 4f, 3f));

        protected readonly FpcBotPlayer botPlayer;

        public override void Tick()
        {
            var spawnPosition = location.Positions[Idx];
            var cameraPosition = botPlayer.BotHub.PlayerHub.PlayerCameraReference.position;

            var dist = Vector3.Distance(spawnPosition, cameraPosition);

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

        public override void Reset()
        { }

        public override string ToString()
        {
            return $"{nameof(GoToLockerSpawnLocation<C>)}({this.Criteria})";
        }
    }
}
