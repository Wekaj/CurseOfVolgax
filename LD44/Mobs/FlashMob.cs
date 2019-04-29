using LD44.Levels;
using LD44.Physics;
using Microsoft.Xna.Framework;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;
using System;

namespace LD44.Mobs {
    public sealed class FlashMob : IMob {
        private float _life = 0.25f;
        private float _rotation = 0f;

        public FlashMob(Random random) {
            if (random.Next(2) == 0) {
                _rotation = 10f;
            }
            else {
                _rotation = -10f;
            }
        }

        public Sprite Sprite { get; } = new Sprite("flash") {
            Origin = new Vector2(0.5f),
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 0.2f, 0.2f)) {
            Ghost = true
        };
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = false;
        public CollisionType CollisionType { get; set; }
        public bool Dead { get; set; }

        public bool Hittable { get; set; }
        public int Health { get; set; }
        public float HitCooldown { get; set; }

        public void Update(LD44Game game, Level level, float delta) {
            Sprite.Rotation += _rotation * delta;

            if (_life > delta) {
                _life -= delta;
            }
            else {
                _life = 0f;
                Dead = true;
            }

            float p = _life / 0.25f;
            float ip = 1f - p;

            Sprite.Scale = new Vector2(ip);
            Sprite.Color = Color.White * (1f - (ip * ip * ip * ip * ip * ip));
        }
    }
}
