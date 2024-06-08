using System.Collections.Generic;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight
{
    internal class ColliderDataComponent : MonoBehaviour
    {
        public Dictionary<Collider, ColliderData> ColliderDatas { get; } = new();

        private Collider[] colliders;

        public void Awake()
        {
            colliders = GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                ColliderDatas[collider] = new ColliderData(collider.GetInstanceID(), collider.bounds.center);
            }
        }

        public void Update()
        {
            foreach (var collider in colliders)
            {
                ColliderDatas[collider] = new ColliderData(ColliderDatas[collider].InstanceId, collider.bounds.center);
            }
        }
    }
}
