using LD44.Levels;
using LD44.Physics;
using Microsoft.Xna.Framework;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;
using System;

namespace LD44.Mobs {
    public sealed class ProjectileMob : IMob {
        private float _life = 10f;

        public Sprite Sprite { get; } = new Sprite("projectile") {
            Origin = new Vector2(0.5f)
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 0.5f, 0.5f)) {
            Ghost = true
        };
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = false;
        public CollisionType CollisionType { get; set; } = CollisionType.Absorb;
        public bool Dead { get; set; }

        public bool Hittable { get; set; } = true;
        public int Health { get; set; } = 1;
        public float HitCooldown { get; set; }

        public void Update(LD44Game game, Level level, float delta) {
            if (_life > delta) {
                _life -= delta;
            }
            else {
                _life = 0f;
                Dead = true;
            }
        }
    }
}
