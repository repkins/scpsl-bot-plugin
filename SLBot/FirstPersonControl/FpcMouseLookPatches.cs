using CursorManagement;
using Discord;
using HarmonyLib;
using PlayerRoles.FirstPersonControl;
using System.Reflection;
using UnityEngine;

namespace TestPlugin.SLBot.FirstPersonControl
{
    [HarmonyPatch(typeof(FpcMouseLook))]
    internal static class FpcMouseLookPatches
    {
        [HarmonyPatch(nameof(FpcMouseLook.UpdateRotation))]
        [HarmonyPrefix()]
        public static bool UpdateBotRotation(FpcMouseLook __instance)
        {
            var hub = HubField.GetValue(__instance) as ReferenceHub;

            if (BotManager.Instance.BotPlayers.TryGetValue(hub, out var botPlayer))
            {
                float vRot = botPlayer.FpcBotPlayer.DesiredLook.x;
                float hRot = botPlayer.FpcBotPlayer.DesiredLook.y;

                __instance.CurrentVertical += vRot;
                __instance.CurrentHorizontal += hRot;
                Quaternion rotation = Quaternion.Euler(Vector3.up * __instance.CurrentHorizontal);
                Quaternion cameraRotation = Quaternion.Euler(Vector3.left * __instance.CurrentVertical);

                hub.transform.rotation = rotation;
                hub.PlayerCameraReference.localRotation = cameraRotation;

                return false;
            }

            return true;
        }

        private static readonly FieldInfo HubField = AccessTools.Field(typeof(FpcMouseLook), "_hub");
    }
}
