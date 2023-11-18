using HarmonyLib;
using InventorySystem.Searching;
using LiteNetLib;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using Mono.Cecil.Cil;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using SCPSLBot.AI;
using SCPSLBot.AI.FirstPersonControl;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace SCPSLBot.LocalNetworking
{
    [HarmonyPatch(typeof(SearchCoordinator))]
    internal static class SearchCoordinatorPatches
    {
        static readonly FieldInfo f_Peers = AccessTools.Field(typeof(LiteNetLib4MirrorServer), nameof(LiteNetLib4MirrorServer.Peers));
        static readonly MethodInfo pg_connectionToClient = AccessTools.PropertyGetter(typeof(SearchCoordinator), nameof(SearchCoordinator.connectionToClient));
        static readonly MethodInfo pg_Ping = AccessTools.PropertyGetter(typeof(NetPeer), nameof(NetPeer.Ping));

        [HarmonyPatch("ReceiveRequestUnsafe")]
        [HarmonyTranspiler()]
        public static IEnumerable<CodeInstruction> SkipPingReadingForBots(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var found = false;
            
            var instructionsEnumerator = instructions.GetEnumerator();
            while (instructionsEnumerator.MoveNext())
            {
                if (instructionsEnumerator.Current.LoadsField(f_Peers) && !found)
                {
                    var loadPeersFieldInstruction = instructionsEnumerator.Current;

                    found = true;

                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, pg_connectionToClient);
                    yield return CodeInstruction.Call<NetworkConnectionToClient, bool>(c => IsConnectionPlayerBot(c));

                    var regularPlayerConnection = generator.DefineLabel();
                    var nextAfterPingGetting = generator.DefineLabel();

                    yield return new CodeInstruction(OpCodes.Brfalse, regularPlayerConnection);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, 1); //
                    yield return new CodeInstruction(OpCodes.Br, nextAfterPingGetting);

                    yield return instructionsEnumerator.Current.WithLabels(regularPlayerConnection);
                    while (instructionsEnumerator.MoveNext() && !instructionsEnumerator.Current.Calls(pg_Ping))
                    {
                        yield return instructionsEnumerator.Current;    // between Peers and Ping
                    }

                    yield return instructionsEnumerator.Current;    // call Ping getter

                    if (instructionsEnumerator.MoveNext())
                    {
                        yield return instructionsEnumerator.Current.WithLabels(nextAfterPingGetting);
                    }
                }
                else
                {
                    yield return instructionsEnumerator.Current;
                }
            }

            if (found is false)
            {
                Log.Error("Cannot find <Ldfld Peers> in SearchCoordinator.ReceiveRequestUnsafe");
            }
        }

        private static bool IsConnectionPlayerBot(NetworkConnectionToClient connection)
        {
            return connection.connectionId < 0;
        }
    }
}
