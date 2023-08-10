using HarmonyLib;
using LiteNetLib;
using Mirror;
using PluginAPI.Core;
using System.Diagnostics;
using YamlDotNet.Core.Tokens;
using System.Net;
using Mirror.LiteNetLib4Mirror;
using System.Net.Sockets;
using System.Reflection;
using System.Collections.Generic;
using LiteNetLib.Utils;
using System;

namespace TestPlugin
{
    [HarmonyPatch()]
    public static class DebugPatches
    {
        //[HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.DisconnectClient))]
        //[HarmonyPrefix()]
        //public static void DebugDisconnectClient(CharacterClassManager __instance, NetworkConnection conn, string message)
        //{
        //    Log.Info($"conn = {conn}");
        //    Log.Info($"message = {message}");

        //    var stackTrace = new StackTrace();
        //    Log.Info($"stackTrace:");
        //    Log.Info($"{stackTrace}");
        //}

        //[HarmonyPatch(typeof(NetworkConnectionToClient), nameof(NetworkConnectionToClient.Disconnect))]
        //[HarmonyPrefix()]
        //public static void DebugDisconnect(NetworkConnectionToClient __instance)
        //{
        //    var error = LiteNetLib4MirrorCore.LastDisconnectError;
        //    var reason = LiteNetLib4MirrorCore.LastDisconnectReason;
        //    Log.Info($"error = {error}");
        //    Log.Info($"reason = {reason}");

        //    var stackTrace = new StackTrace();
        //    Log.Info($"stackTrace:");
        //    Log.Info($"{stackTrace}");
        //}
    }


    //[HarmonyPatch(typeof(NetManager))]
    //public static class NetManagerPatches
    //{
    //    [HarmonyTargetMethod]
    //    public static MethodBase GetDisconnectPeerForceMethod()
    //    {
    //        var netPacketType = Assembly.GetAssembly(typeof(NetPeer)).GetType("LiteNetLib.NetPacket");
    //        return AccessTools.Method(typeof(NetManager), "DisconnectPeerForce", new Type[] { typeof(NetPeer), typeof(DisconnectReason), typeof(SocketError), netPacketType });
    //    }

    //    [HarmonyPrefix()]
    //    public static void DebugDisconnectPeerForce(NetManager __instance)
    //    {
    //        var stackTrace = new StackTrace();
    //        Log.Info($"stackTrace:");
    //        Log.Info($"{stackTrace}");
    //    }
    //}
}
