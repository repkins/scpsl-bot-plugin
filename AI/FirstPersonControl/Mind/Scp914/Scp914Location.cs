using Interactables;
using MapGeneration;
using Scp914;
using SCPSLBot.AI.FirstPersonControl.Mind.Spacial;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using SCPSLBot.Navigation.Mesh;
using System.Linq;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Scp914
{
    internal class Scp914Location : Location
    {
        public Scp914Location(RoomSightSense roomSightSense)
        {
            roomSightSense.OnSensedForeignRoomArea += OnSensedRoom;
        }

        private void OnSensedRoom(Area foreignRoomArea)
        {
            var foreignRoom = foreignRoomArea.Room.Identifier;
            if (foreignRoom.Name != RoomName.Lcz914)
            {
                return;
            }

            this.Update(Scp914Controller.Singleton);
        }

        public Vector3? IntakeChamberPosition { get; private set; }
        public Vector3? OutakeChamberPosition { get; private set; }
        public Vector3? ControlsPosition { get; private set; }
        public Vector3? SettingKnobPosition { get; private set; }
        public Vector3? StartKnobPosition { get; private set; }

        private void Update(Scp914Controller controller)
        {
            var scp914position = controller.transform.position;
            if (!this.Positions.Contains(scp914position))
            {
                this.AddPosition(scp914position);
                this.IntakeChamberPosition = controller.IntakeChamber.position;
                this.OutakeChamberPosition = controller.OutputChamber.position;

                var interactableColliderPositions = InteractableCollider.AllInstances[controller]
                    .ToDictionary(pair => (Scp914InteractCode)pair.Key, pair => pair.Value.GetComponent<Collider>().bounds.center);

                this.SettingKnobPosition = interactableColliderPositions[Scp914InteractCode.ChangeMode];
                this.StartKnobPosition = interactableColliderPositions[Scp914InteractCode.Activate];
                this.ControlsPosition = this.StartKnobPosition;
            }
        }

        public override string ToString()
        {
            return $"{nameof(Scp914Location)}: {Positions.Count > 0}";
        }
    }
}
