using LD44.Levels;
using LD44.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;

namespace LD44.Mobs {
    public sealed class PlayerMob : IMob {
        public Sprite Sprite { get; } = new Sprite("player") {
            Offset = new Vector2(-8f, -11f)
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 0.25f, 0.6f));
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = true;
        public CollisionType CollisionType { get; set; }
        public float LenienceTimer { get; set; }
        public float StunTimer { get; set; }
        public float CollisionCooldownTimer { get; set; }
        public bool Dead { get; set; }
        public bool FacingRight => Sprite.Effects == SpriteEffects.None;

        public bool Hittable { get; set; }
        public int Health { get; set; }
        public float HitCooldown { get; set; }

        public void Update(LD44Game game, Level level, float delta) {
        }
    }
}
