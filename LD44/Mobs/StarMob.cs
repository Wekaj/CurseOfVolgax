using LD44.Physics;
using Microsoft.Xna.Framework;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;

namespace LD44.Mobs {
    public sealed class StarMob : IMob {
        public Sprite Sprite { get; } = new Sprite("star") {
            Offset = new Vector2(-3.5f, -3f)
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 0.2f, 0.2f)) {
            Bouncy = true,
            BounceFactor = 0.8f
        };
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = false;
        public CollisionType CollisionType { get; set; }

        public void Update(float delta) {
        }
    }
}
