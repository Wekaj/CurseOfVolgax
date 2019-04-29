using LD44.Levels;
using LD44.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;
using System;
using System.Linq;

namespace LD44.Mobs {
    public sealed class EliteBatMob : IMob {
        private Random _random = new Random();
        private bool _sighted = false;
        private bool _preparing = false;
        private float _prepareTimer = 0f;
        private bool _charging = false;
        private float _chargeTimer = 0f;

        public Sprite Sprite { get; } = new Sprite("elite_bat") {
            Offset = new Vector2(-12f, -12f)
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 0.5f, 0.5f)) {
            Ghost = true
        };
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = false;
        public CollisionType CollisionType { get; set; } = CollisionType.Stun;
        public bool Dead { get; set; }

        public bool FacingRight => Sprite.Effects == SpriteEffects.FlipHorizontally;

        public bool Hittable { get; set; } = true;
        public int Health { get; set; } = 30;
        public float HitCooldown { get; set; }

        public void Update(LD44Game game, Level level, float delta) {
            if (_charging) {
                _chargeTimer -= delta;
                if (_chargeTimer <= 0f) {
                    _charging = false;
                }

                if (Body.Velocity != Vector2.Zero) {
                    float p = _chargeTimer / 0.5f;
                    float ip = 1f - p;

                    var dir = Vector2.Normalize(Body.Velocity);
                    Body.Velocity = dir * 10f * (1f - ip * ip * ip * ip);
                }

                return;
            }

            PlayerMob playerMob = level.Mobs.OfType<PlayerMob>().First();

            Body.Velocity = Vector2.Zero;

            if (!_sighted) {
                if (Vector2.Distance(playerMob.Body.Position, Body.Position) < 10f) {
                    _sighted = true;
                }
            }
            else if (playerMob.Body.Position != Body.Position) {
                float distance = Vector2.Distance(playerMob.Body.Position, Body.Position);

                if (_preparing) {
                    if (distance > 5f) {
                        _preparing = false;
                    }
                    else {
                        _prepareTimer += delta;

                        if (_prepareTimer > 2f) {
                            var flash = new FlashMob(_random);
                            flash.Body.Position = Body.Position;
                            level.FutureMobs.Add(flash);

                            _charging = true;
                            _chargeTimer = 0.5f;

                            Vector2 dir = (playerMob.Body.Position - Body.Position) / distance;
                            Body.Velocity = dir * 10f;

                            _prepareTimer = 0f;
                        }
                    }
                }
                else if (distance > 2f) {
                    Vector2 dir = (playerMob.Body.Position - Body.Position) / distance;
                    Body.Velocity = dir * 4f;

                    if (_prepareTimer > 0f) {
                        _prepareTimer -= delta;
                    }
                }
                else {
                    _preparing = true;
                }

                if (playerMob.Body.Position.X > Body.Position.X) {
                    Sprite.Effects = SpriteEffects.FlipHorizontally;
                }
                else if (playerMob.Body.Position.X < Body.Position.X) {
                    Sprite.Effects = SpriteEffects.None;
                }
            }

            if (Body.Velocity.X > 0f) {
                Sprite.Effects = SpriteEffects.FlipHorizontally;
            }
            else if (Body.Velocity.X < 0f) {
                Sprite.Effects = SpriteEffects.None;
            }
        }
    }
}
