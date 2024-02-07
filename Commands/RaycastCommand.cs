using CommandSystem;
using MapGeneration;
using Mirror;
using PlayerRoles;
using PluginAPI.Core;
using RemoteAdmin;
using System;
using UnityEngine;

namespace TestPlugin.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    internal class RaycastCommand : ICommand
    {
        public string Command => "raycast";

        public string[] Aliases => new string[] { };

        public string Description => "Casts ray among camera direction from camera and prints hit results.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerCommandSender)
            {
                response = "You must be in-game to use this command!";
                return false;
            }

            if (!playerCommandSender.ReferenceHub.IsAlive())
            {
                response = "Command disabled when you are not alive!";
                return false;
            }

            if (!Physics.Raycast(playerCommandSender.ReferenceHub.PlayerCameraReference.position, playerCommandSender.ReferenceHub.PlayerCameraReference.forward, out var hit, 20f))
            {
                response = "Raycast produced no any hit within max distance.";
                return false;
            }
            
            var collider = hit.collider;
            var layerName = LayerMask.LayerToName(collider.gameObject.layer);

            response = $"Got hit with collider of {collider.gameObject} and layer {layerName}";
            Log.Info(response);

            return true;
        }
    }
}
