using AdminToys;
using MapGeneration;
using MEC;
using Mirror;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace SCPSLBot.Navigation.Graph
{
    internal class NavigationGraphVisualsPool
    {
        public static NavigationGraphVisualsPool Instance { get; } = new();

        private ObjectPool<PrimitiveObjectToy> vertexVisualsPool = new ObjectPool<PrimitiveObjectToy>(CreateVisual);
        private ObjectPool<PrimitiveObjectToy> areaVisualsPool = new ObjectPool<PrimitiveObjectToy>(CreateVisual);
        private ObjectPool<PrimitiveObjectToy> edgeVisualsPool = new ObjectPool<PrimitiveObjectToy>(CreateVisual);

        private PrimitiveObjectToy primPrefab;

        public void Init()
        {
            EventManager.RegisterEvents(this);
        }

        [PluginEvent(PluginAPI.Enums.ServerEventType.MapGenerated)]
        public void AssignPrimPrefab()
        {
            this.primPrefab = NetworkClient.prefabs.Values.Select(p => p.GetComponent<PrimitiveObjectToy>()).First(p => p);
        }

        public PrimitiveObjectToy GetVertexVisualFromPool()
        {
            return vertexVisualsPool.Get();
        }

        public void ReturnVertexVisualToPool(PrimitiveObjectToy visual)
        {
            vertexVisualsPool.Release(visual);
        }

        public PrimitiveObjectToy GetAreaVisualFromPool()
        {
            return areaVisualsPool.Get();

        }

        public void ReturnAreaVisualToPool(PrimitiveObjectToy visual)
        {
            areaVisualsPool.Release(visual);
        }

        public PrimitiveObjectToy GetEdgeVisualFromPool()
        {
            return edgeVisualsPool.Get();
        }

        public void ReturnEdgeVisualToPool(PrimitiveObjectToy visual)
        {
            edgeVisualsPool.Release(visual);
        }

        private static PrimitiveObjectToy CreateVisual()
        {
            var newVisual = UnityEngine.Object.Instantiate(Instance.primPrefab);
            NetworkServer.Spawn(newVisual.gameObject);
            return newVisual;
        }

        private static void OnGet()
        {

        }
        private static void OnRelease()
        {

        }
        private static void OnDestroy(PrimitiveObjectToy visual)
        {
            NetworkServer.Destroy(visual.gameObject);

        }
    }
}
