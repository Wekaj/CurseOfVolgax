using LD44.Levels;
using LD44.Physics;
using Microsoft.Xna.Framework;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;

namespace LD44.Mobs {
    public sealed class SpikesMob : IMob {
        public Sprite Sprite { get; } = new Sprite("spikes") {
            Offset = new Vector2(-8f, -14f)
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 1f, 0.3f));
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = false;
        public CollisionType CollisionType { get; set; } = CollisionType.SpikeKill;
        public bool Dead { get; set; }

        public bool Hittable { get; set; }
        public int Health { get; set; }
        public float HitCooldown { get; set; }

        public void Update(LD44Game game, Level level, float delta) {
        }
    }
}
