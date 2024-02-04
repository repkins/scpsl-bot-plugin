using PlayerRoles;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses
{
    internal class PlayersWithinSightSense : SightSense, ISense
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

        public void Reset()
        {
            PlayersWithinSight.Clear();
        }

        public void ProcessSensibility(Collider collider)
        {
            if (collider.GetComponentInParent<ReferenceHub>() is ReferenceHub otherPlayer
                && otherPlayer != _fpcBotPlayer.BotHub.PlayerHub
                && !PlayersWithinSight.Contains(otherPlayer))
            {
                if (IsWithinSight(collider, otherPlayer))
                {
                    PlayersWithinSight.Add(otherPlayer);
                }
            }
        }

        public void UpdateBeliefs()
        {
        }

        private readonly FpcBotPlayer _fpcBotPlayer;
    }
}
