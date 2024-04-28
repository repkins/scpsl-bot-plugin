using System;

namespace SCPSLBot.Navigation.Mesh
{
    internal struct RoomKindEdge
    {
        public RoomKindVertex From;
        public RoomKindVertex To;

        public RoomKindEdge(RoomKindVertex from, RoomKindVertex to)
        {
            From = from;
            To = to;
        }
        public override bool Equals(object obj)
        {
            return obj is RoomKindEdge edge && (From, To).Equals((edge.From, edge.To));
        }

        public override int GetHashCode()
        {
            return (From, To).GetHashCode();
        }

        public static bool operator ==(RoomKindEdge left, RoomKindEdge right)
        {
            return (left.From, left.To) == (right.From, right.To);
        }

        public static bool operator !=(RoomKindEdge left, RoomKindEdge right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return (From, To).ToString();
        }
    }
}
