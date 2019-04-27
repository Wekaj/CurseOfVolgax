using Microsoft.Xna.Framework;
using Ruut;

namespace LD44.Physics {
    public sealed class Body {
        public Body(RectangleF bounds) {
            Bounds = bounds;
        }

        public RectangleF Bounds { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public Vector2 Contact { get; set; }
    }
}
