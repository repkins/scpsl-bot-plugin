using Interactables;
using MapGeneration;
using RemoteAdmin;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class Scp914Location : IBelief
    {
        public Scp914Location(RoomSightSense roomSightSense)
        {
            roomSightSense.OnSensedRoomWithin += OnSensedRoomWithin;
        }

        private void OnSensedRoomWithin(RoomIdentifier roomWithin)
        {
            if (roomWithin.Name != RoomName.Lcz914)
            {
                return;
            }

            this.Update(Scp914Controller.Singleton);
        }

        public Vector3? Position { get; private set; }
        public Vector3? IntakeChamberPosition { get; private set; }
        public Vector3? ControlsPosition { get; private set; }
        public Vector3? SettingKnobPosition { get; private set; }
        public Vector3? StartKnobPosition { get; private set; }
        public event Action OnUpdate;

        private void Update(Scp914Controller controller)
        {
            var newPosition = controller.transform.position;
            if (newPosition != this.Position)
            {
                this.Position = newPosition;
                this.IntakeChamberPosition = controller.IntakeChamber.position;

                var interactableColliderPositions = controller.GetComponentsInChildren<InteractableCollider>()
                    .ToDictionary(ic => (Scp914InteractCode)ic.ColliderId, ic => ic.GetComponent<Collider>().bounds.center);

                this.SettingKnobPosition = interactableColliderPositions[Scp914InteractCode.ChangeMode];
                this.StartKnobPosition = interactableColliderPositions[Scp914InteractCode.Activate];
                this.ControlsPosition = this.StartKnobPosition;

                this.OnUpdate?.Invoke();
            }
        }

        public override string ToString()
        {
            return $"{nameof(Scp914Location)}";
        }
    }
}
