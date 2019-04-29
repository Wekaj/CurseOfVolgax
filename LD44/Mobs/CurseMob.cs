using LD44.Levels;
using LD44.Physics;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;

namespace LD44.Mobs {
    public sealed class CurseMob : IMob {
        public Sprite Sprite { get; } = new Sprite("empty");

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 1f, 1f)) {
            Ghost = true
        };
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = false;
        public CollisionType CollisionType { get; set; } = CollisionType.Curse;
        public bool Dead { get; set; }

        public bool Hittable { get; set; }
        public int Health { get; set; }
        public float HitCooldown { get; set; }

        public void Update(LD44Game game, Level level, float delta) {
        }
    }
}
