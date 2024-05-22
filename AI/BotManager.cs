using CentralAuth;
using MEC;
using Mirror;
using PlayerRoles;
using PluginAPI.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using LocalConnectionToClient = SCPSLBot.LocalNetworking.LocalConnectionToClient;
using LocalConnectionToServer = SCPSLBot.LocalNetworking.LocalConnectionToServer;

namespace SCPSLBot.AI
{
    internal class BotManager
    {
        public static BotManager Instance { get; } = new BotManager();

        public Dictionary<ReferenceHub, BotHub> BotPlayers { get; } = new Dictionary<ReferenceHub, BotHub>();

        public void Init()
        {
            Timing.RunCoroutine(RunPlayerUpdates());

            PlayerRoleManager.OnRoleChanged += OnRoleChanged;
        }

        public void AddBotPlayer()
        {
            var gameObject = Object.Instantiate(NetworkManager.singleton.playerPrefab);
            gameObject.name = string.Format("{0} [bot]", NetworkManager.singleton.playerPrefab.name);

            var connectionToClient = new LocalConnectionToClient(--lastConnNum);
            var connectionToServer = new LocalConnectionToServer() { connectionToClient = connectionToClient };
            connectionToClient.connectionToServer = connectionToServer;

            Log.Info($"connectionToClient = {connectionToClient}");
            //NetworkDiagnostics.InMessageEvent += LogInMessage;

            NetworkServer.AddConnection(connectionToClient);
            NetworkServer.AddPlayerForConnection(connectionToClient, gameObject);
            NetworkServer.OnConnectedEvent?.Invoke(connectionToClient);

            var referenceHub = gameObject.GetComponent<ReferenceHub>();

            BotPlayers.Add(referenceHub, new BotHub(connectionToClient, connectionToServer, referenceHub));

            Log.Info($"connectionToClient.identity = {connectionToClient.identity}");

            Timing.RunCoroutine(AssignUserIdAsync(gameObject));
        }

        private IEnumerator<float> AssignUserIdAsync(GameObject player)
        {
            yield return Timing.WaitForSeconds(1f);

            PlayerAuthenticationManager playerAuthManager = player.GetComponent<PlayerAuthenticationManager>();
            playerAuthManager.UserId = $"BotUserId{this.lastConnNum}";

            yield break;
        }

        public IEnumerator<float> RunPlayerUpdates()
        {
            while (true)
            {
                foreach (var player in BotPlayers.Values)
                {
                    player.Update();
                }

                yield return Timing.WaitForOneFrame;
            }
        }

        public void OnRoleChanged(ReferenceHub userHub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
        {
            if (BotPlayers.TryGetValue(userHub, out var botPlayer))
            {
                botPlayer.OnRoleChanged(prevRole, newRole);
            }
        }

        public void DebugBotMindDump(int playerId)
        {
            if (!ReferenceHub.TryGetHub(playerId, out var hub))
            {
                Log.Warning($"There is no player with such id.");
                return;
            }

            if (!BotPlayers.TryGetValue(hub, out var botPlayer))
            {
                Log.Warning($"Player with such id is not a bot.");
                return;
            }

            botPlayer.MindDump();
        }

        public void DebugBotMove(int playerId, Vector3 direction, int timeAmount)
        {
            if (!ReferenceHub.TryGetHub(playerId, out var hub))
            {
                Log.Warning($"There is no player with such id.");
                return;
            }

            if (!BotPlayers.TryGetValue(hub, out var botPlayer))
            {
                Log.Warning($"Player with such id is not a bot.");
                return;
            }

            Timing.RunCoroutine(botPlayer.MoveAsync(direction, timeAmount));
        }

        public void DebugBotTurn(int playerId, Vector3 degrees, Vector3 targetDegrees)
        {
            if (!ReferenceHub.TryGetHub(playerId, out var hub))
            {
                Log.Warning($"There is no player with such id.");
                return;
            }

            if (!BotPlayers.TryGetValue(hub, out var botPlayer))
            {
                Log.Warning($"Player with such id is not a bot.");
                return;
            }

            Timing.RunCoroutine(botPlayer.TurnAsync(degrees, targetDegrees));
        }

        public void DebugBotApproach(int playerId)
        {
            if (!ReferenceHub.TryGetHub(playerId, out var hub))
            {
                Log.Warning($"There is no player with such id.");
                return;
            }

            if (!BotPlayers.TryGetValue(hub, out var botPlayer))
            {
                Log.Warning($"Player with such id is not a bot.");
                return;
            }

            Timing.RunCoroutine(botPlayer.ApproachAsync());
        }

        private BotManager()
        { }

        private int lastConnNum = 0;
    }
}
