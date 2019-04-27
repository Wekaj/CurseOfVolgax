using System;

namespace LD44.Generation {
    public enum SideStatus {
        Undecided,
        Open,
        Closed,
        Edge
    }

    public struct ChunkSides : IEquatable<ChunkSides> {
        public SideStatus Left { get; set; }
        public SideStatus Right { get; set; }
        public SideStatus Top { get; set; }
        public SideStatus Bottom { get; set; }

        public override bool Equals(object obj) {
            return obj is ChunkSides sides && Equals(sides);
        }

        public bool Equals(ChunkSides other) {
            return Left == other.Left &&
                   Right == other.Right &&
                   Top == other.Top &&
                   Bottom == other.Bottom;
        }

        public override int GetHashCode() {
            var hashCode = 551583723;
            hashCode = hashCode * -1521134295 + Left.GetHashCode();
            hashCode = hashCode * -1521134295 + Right.GetHashCode();
            hashCode = hashCode * -1521134295 + Top.GetHashCode();
            hashCode = hashCode * -1521134295 + Bottom.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(ChunkSides left, ChunkSides right) {
            return left.Equals(right);
        }

        public static bool operator !=(ChunkSides left, ChunkSides right) {
            return !(left == right);
        }
    }
}
