using HarmonyLib;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
using System;
using System.Reflection;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Rotation
{
    [HarmonyPatch(typeof(FpcMouseLook))]
    internal static class FpcMouseLookPatches
    {
        [HarmonyPatch(nameof(FpcMouseLook.UpdateRotation))]
        [HarmonyPrefix()]
        public static bool UpdateBotRotation(FpcMouseLook __instance)
        {
            var hub = HubField.GetValue(__instance) as ReferenceHub;

            if (hub != null && BotManager.Instance.BotPlayers.TryGetValue(hub, out var botHub)
                && botHub.CurrentBotPlayer is FpcBotPlayer fpcPlayer)
            {
                var hRotation = fpcPlayer.Look.GoaldHorizontalRotation;
                var vRotation = fpcPlayer.Look.GoaldVerticalRotation;

                //__instance.CurrentHorizontal = Mathf.DeltaAngle(0f, hub.transform.eulerAngles.y);
                //__instance.CurrentVertical = -Mathf.DeltaAngle(0f, hub.PlayerCameraReference.localEulerAngles.x);

                __instance.CurrentHorizontal += hRotation.eulerAngles.y;
                __instance.CurrentVertical += -Mathf.DeltaAngle(0f, vRotation.eulerAngles.x);

                Quaternion rotation = Quaternion.Euler(Vector3.up * __instance.CurrentHorizontal);
                Quaternion cameraRotation = Quaternion.Euler(Vector3.left * __instance.CurrentVertical);

                hub.transform.rotation = rotation;
                hub.PlayerCameraReference.localRotation = cameraRotation;

                fpcPlayer.Look.GoaldHorizontalRotation = Quaternion.identity;
                fpcPlayer.Look.GoaldVerticalRotation = Quaternion.identity;

                return false;
            }

            return true;
        }

        private static readonly FieldInfo HubField = AccessTools.Field(typeof(FpcMouseLook), "_hub");
    }
}
