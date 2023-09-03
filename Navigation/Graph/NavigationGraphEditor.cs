using AdminToys;
using MapGeneration;
using MEC;
using Mirror;
using PluginAPI.Core;
using PluginAPI.Events;
using PluginAPI.Loader;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLBot.Navigation.Graph
{
    internal class NavigationGraphEditor
    {
        public bool IsEditing { get; set; }
        public Player PlayerEditing { get; set; }

        public static NavigationGraphEditor Instance { get; } = new NavigationGraphEditor();

        private NavigationGraph NavigationGraph { get; } = NavigationGraph.Instance;
        private NavigationGraphVisuals Visuals { get; } = new NavigationGraphVisuals();

        private Player LastPlayerEditing { get; set; }

        public void Init()
        {
            Timing.RunCoroutine(RunEachFrame(UpdateEditing));
            Timing.RunCoroutine(RunEachFrame(UpdateNearestNode));
            Timing.RunCoroutine(RunEachFrame(UpdateFacingNode));
            Timing.RunCoroutine(RunEachFrame(Visuals.UpdateNodeInfoVisuals));
            Timing.RunCoroutine(RunEachFrame(Visuals.UpdateNodeVisuals));
        }

        public RoomKindNode FindClosestNodeFacingAt((RoomName, RoomShape) roomNameShape, Vector3 localPosition, Vector3 localDirection)
        {
            var targetNode = NavigationGraph.Instance.NodesByRoomKind[roomNameShape]
                .Select(n => (n, d: Vector3.SqrMagnitude(n.LocalPosition - localPosition)))
                .Where(t => t.d < 50f && t.d > 1f)
                .OrderBy(t => t.d)
                .Select(t => t.n)
                .FirstOrDefault(n => Vector3.Dot(Vector3.Normalize(n.LocalPosition - localPosition), localDirection) > 0.999848f);

            return targetNode;
        }

        public RoomKindNode AddNode(Vector3 position)
        {
            var room = RoomIdUtils.RoomAtPositionRaycasts(position);

            var newNode = NavigationGraph.AddNode(room.transform.InverseTransformPoint(position), (room.Name, room.Shape));

            Log.Info($"Node #{newNode.Id} at local position {newNode.LocalPosition} added under room {(room.Name, room.Shape)}.");

            return newNode;
        }

        public void RemoveNode(Vector3 position)
        {
            var node = NavigationGraph.FindNearestNode(position);
            if (node == null)
            {
                Log.Warning($"No node found nearby to remove.");

                return;
            }

            var room = RoomIdUtils.RoomAtPositionRaycasts(position);

            NavigationGraph.RemoveNode(node.RoomKindNode, (room.Name, room.Shape));

            Log.Info($"Node at local position {node.LocalPosition} removed under room {(room.Name, room.Shape)}.");
        }

        public void UpdateEditing()
        {
            if (PlayerEditing != LastPlayerEditing)
            {
                LastPlayerEditing = PlayerEditing;

                Visuals.EnabledVisualsForPlayer = PlayerEditing;
            }
        }

        public void UpdateNearestNode()
        {
            if (PlayerEditing != null)
            {
                Visuals.NearestNodeTemplate = NavigationGraph.FindNearestNode(PlayerEditing.Camera.position)?.RoomKindNode;
            }
        }

        public void UpdateFacingNode()
        {
            if (PlayerEditing != null)
            {
                var room = RoomIdUtils.RoomAtPositionRaycasts(PlayerEditing.Position);

                var localPosition = room.transform.InverseTransformPoint(PlayerEditing.Camera.position);
                var localForward = room.transform.InverseTransformDirection(PlayerEditing.Camera.forward);

                Visuals.FacingNodeTemplate = FindClosestNodeFacingAt((room.Name, room.Shape), localPosition, localForward);
            }
        }

        #region Private constructor
        private NavigationGraphEditor()
        { }
        #endregion

        private IEnumerator<float> RunEachFrame(Action action)
        {
            while (true)
            {
                action.Invoke();

                yield return Timing.WaitForOneFrame;
            }
        }
    }
}
