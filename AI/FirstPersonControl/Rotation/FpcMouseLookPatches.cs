using HarmonyLib;
using PlayerRoles.FirstPersonControl;
using PluginAPI.Core;
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

            if (BotManager.Instance.BotPlayers.TryGetValue(hub, out var botHub)
                && botHub.CurrentBotPlayer is FpcBotPlayer fpcPlayer)
            {
                var hRotation = fpcPlayer.Look.DesiredHorizontalRotation;
                var vRotation = fpcPlayer.Look.DesiredVerticalRotation;

                hub.transform.rotation *= hRotation;
                hub.PlayerCameraReference.localRotation *= vRotation;

                __instance.CurrentHorizontal = hub.transform.eulerAngles.y;
                __instance.CurrentVertical = -Mathf.DeltaAngle(0f, hub.PlayerCameraReference.localEulerAngles.x);

                fpcPlayer.Look.DesiredHorizontalRotation = Quaternion.identity;
                fpcPlayer.Look.DesiredVerticalRotation = Quaternion.identity;

                return false;
            }

            return true;
        }

        private static readonly FieldInfo HubField = AccessTools.Field(typeof(FpcMouseLook), "_hub");
    }
}
