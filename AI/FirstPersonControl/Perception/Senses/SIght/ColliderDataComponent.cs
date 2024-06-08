using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Perception.Senses.Sight
{
    internal class ColliderDataComponent : MonoBehaviour
    {
        public ColliderData ColliderData { get; private set; }

        private Collider collider;

        public void Awake()
        {
            collider = GetComponent<Collider>();
            ColliderData = new ColliderData(collider.GetInstanceID(), collider.bounds.center);
        }

        public void Update()
        {
            ColliderData = new ColliderData(ColliderData.InstanceId, collider.bounds.center);
        }
    }
}
