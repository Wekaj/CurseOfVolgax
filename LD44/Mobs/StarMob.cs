using LD44.Levels;
using LD44.Physics;
using Microsoft.Xna.Framework;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;
using System;

namespace LD44.Mobs {
    public sealed class StarMob : IMob {
        private float _life = 2f;

        public Sprite Sprite { get; } = new Sprite("star") {
            Origin = new Vector2(0.5f)
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 0.2f, 0.2f)) {
            Bouncy = true,
            BounceFactor = 0.8f
        };
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = true;
        public CollisionType CollisionType { get; set; }
        public bool Dead { get; set; }

        public bool Hittable { get; set; }
        public int Health { get; set; }
        public float HitCooldown { get; set; }

        public void Update(LD44Game game, Level level, float delta) {
            if (Body.Velocity.X < 0f) {
                Sprite.Rotation -= 5f * delta;
            }
            else {
                Sprite.Rotation += 5f * delta;
            }

            if (_life > delta) {
                _life -= delta;
            }
            else {
                _life = 0f;
                Dead = true;
            }

            float p = _life / 2f;

            Sprite.Scale = new Vector2(1f - (float)Math.Pow(1f - p, 5f));
        }
    }
}
