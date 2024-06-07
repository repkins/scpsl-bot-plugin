using Interactables;
using Interactables.Interobjects.DoorUtils;
using PlayerRoles;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class PlayersWithinSightSense : SightSense<ReferenceHub>
    {
        public HashSet<ReferenceHub> PlayersWithinSight => ComponentsWithinSight;
        public IEnumerable<ReferenceHub> EnemiesWithinSight { get; }
        public IEnumerable<ReferenceHub> FriendiesWithinSight { get; }

        public PlayersWithinSightSense(FpcBotPlayer botPlayer) : base(botPlayer)
        {
            _fpcBotPlayer = botPlayer;

            EnemiesWithinSight = PlayersWithinSight.Where(o => o.GetFaction() != botPlayer.BotHub.PlayerHub.GetFaction())
                                                    .Where(o => o.GetFaction() != Faction.Unclassified);
            FriendiesWithinSight = PlayersWithinSight.Where(o => o.GetFaction() == botPlayer.BotHub.PlayerHub.GetFaction());
        }

        private LayerMask hitboxLayerMask = LayerMask.GetMask("Hitbox");
        protected override LayerMask LayerMask => hitboxLayerMask;

        public override void ProcessSightSensedItems() { }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
