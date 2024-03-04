using MapGeneration;
using SCPSLBot.AI.FirstPersonControl.Perception.Senses;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Beliefs.Scp914
{
    internal class Scp914Chamber : IBelief
    {
        public readonly Side Side;
        public Scp914Chamber(Side side)
        {
            this.Side = side;
        }

        public bool Inside => Side == Side.In;
        public bool Outside => Side == Side.Out;

        public bool IsPlayerAtSide { get; private set; }

        public event Action OnUpdate;

        public bool Condition()
        {
            return IsPlayerAtSide;
        }

        public Scp914Chamber(SpatialSense spatialSense)
        {
            spatialSense.OnSensedPlayerPosition += ProcessSensedPlayerPosition;
        }

        private void ProcessSensedPlayerPosition(Vector3 playerPosition)
        {
            EnsureAssignedScp914Room();

            Side SidePlayerAt;

            var room = RoomIdUtils.RoomAtPositionRaycasts(playerPosition);
            if (room == scp914RoomInstance && sideLocalPlane.GetSide(room.transform.InverseTransformPoint(playerPosition)) == true)
            {
                SidePlayerAt = Side.In;
            }
            else
            {
                SidePlayerAt = Side.Out;
            }

            var isAtCurrentSide = Side == SidePlayerAt;
            if (isAtCurrentSide != IsPlayerAtSide)
            {
                Update(isAtCurrentSide);
            }
        }

        private void Update(bool isAtSide)
        {
            IsPlayerAtSide = isAtSide;
            OnUpdate?.Invoke();
        }

        private void EnsureAssignedScp914Room()
        {
            if (!scp914RoomInstance)
            {
                RoomIdUtils.TryFindRoom(RoomName.Lcz914, FacilityZone.LightContainment, RoomShape.Endroom, out var foundRoom);
                if (foundRoom)
                {
                    scp914RoomInstance = foundRoom;
                }
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}(IsInside = {IsPlayerAtSide})";
        }

        private static Plane sideLocalPlane = new(Vector3.right, new Vector3(-2.8f, 1f, 0f));
        public Vector3 InsideNormal => scp914RoomInstance.transform.TransformDirection(sideLocalPlane.normal);

        private RoomIdentifier scp914RoomInstance;
    }

    enum Side
    {
        In,
        Out,
    }
}
