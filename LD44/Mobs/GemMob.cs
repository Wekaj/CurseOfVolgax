using LD44.Levels;
using LD44.Physics;
using Microsoft.Xna.Framework;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;

namespace LD44.Mobs {
    public sealed class GemMob : IMob {
        public Sprite Sprite { get; } = new Sprite("gem") {
            Offset = new Vector2(-8f, -8f)
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 0.5f, 0.6f));
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = false;
        public CollisionType CollisionType { get; set; } = CollisionType.Gem;
        public bool Dead { get; set; }

        public bool Hittable { get; set; }
        public int Health { get; set; }
        public float HitCooldown { get; set; }

        public void Update(LD44Game game, Level level, float delta) {
        }
    }
}
