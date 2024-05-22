using Interactables;
using Interactables.Interobjects.DoorUtils;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class PlayersWithinSightSense : SightSense
    {
        public HashSet<ReferenceHub> PlayersWithinSight { get; } = new HashSet<ReferenceHub>();
        public IEnumerable<ReferenceHub> EnemiesWithinSight { get; }
        public IEnumerable<ReferenceHub> FriendiesWithinSight { get; }

        public PlayersWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;

            EnemiesWithinSight = PlayersWithinSight.Where(o => o.GetFaction() != botPlayer.BotHub.PlayerHub.GetFaction())
                                                    .Where(o => o.GetFaction() != Faction.Unclassified);
            FriendiesWithinSight = PlayersWithinSight.Where(o => o.GetFaction() == botPlayer.BotHub.PlayerHub.GetFaction());
        }

        public override void Reset()
        {
            PlayersWithinSight.Clear();
        }

        private Dictionary<Collider, ReferenceHub> collidersOfComponent = new();

        public override void ProcessSensibility(IEnumerable<Collider> colliders)
        {
            Profiler.BeginSample($"{nameof(PlayersWithinSightSense)}.{nameof(ProcessSensibility)}");

            collidersOfComponent.Clear();
            foreach (var collider in colliders)
            {
                if ((hitboxLayerMask & (1 << collider.gameObject.layer)) != 0 
                    && collider.GetComponentInParent<ReferenceHub>() is ReferenceHub otherPlayer 
                    && otherPlayer != _fpcBotPlayer.BotHub.PlayerHub)
                {
                    collidersOfComponent.Add(collider, otherPlayer);
                }
            }

            var withinSight = this.GetWithinSight(collidersOfComponent.Keys);

            foreach (var collider in withinSight)
            {
                PlayersWithinSight.Add(collidersOfComponent[collider]);
            }

            Profiler.EndSample();
        }

        private LayerMask hitboxLayerMask = LayerMask.GetMask("Hitbox");

        public override void ProcessSightSensedItems() { }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
