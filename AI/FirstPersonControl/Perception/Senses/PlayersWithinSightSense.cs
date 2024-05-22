using Interactables;
using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        public override void ProcessSensibility(IEnumerable<Collider> colliders)
        {
            var collidersOfComponent = colliders
                .Select(c => (c, Component: c.GetComponentInParent<ReferenceHub>()))
                .Where(t => t.Component is not null && t.Component != _fpcBotPlayer.BotHub.PlayerHub && !PlayersWithinSight.Contains(t.Component));

            var withinSight = this.GetWithinSight(collidersOfComponent);

            foreach (var collider in withinSight)
            {
                PlayersWithinSight.Add(collider.GetComponentInParent<ReferenceHub>());
            }
        }

        public override void ProcessSightSensedItems() { }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
