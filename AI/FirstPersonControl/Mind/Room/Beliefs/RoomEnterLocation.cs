using SCPSLBot.AI.FirstPersonControl.Mind;
using System;
using UnityEngine;

namespace SCPSLBot.AI.FirstPersonControl.Mind.Room.Beliefs
{
    internal class RoomEnterLocation : IBelief
    {
        public Vector3? Position { get; internal set; }

        public event Action OnUpdate;
    }
}
