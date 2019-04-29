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
    public sealed class MonkeyMob : IMob {
        private enum State {
            Waiting,
            Running,
            Leaping
        }

        private readonly Random _random;

        private bool _sighted = false;
        private float _sentryTimer = 0f;

        private State _state = State.Waiting;
        private float _stateTimer = 0f;

        public MonkeyMob(Random random) {
            _random = random;
        }

        public Sprite Sprite { get; } = new Sprite("monkey") {
            Offset = new Vector2(-8f, -8f)
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 0.8f, 1f));
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = true;
        public CollisionType CollisionType { get; set; } = CollisionType.Stun;
        public bool Dead { get; set; }

        public bool FacingRight => Sprite.Effects == SpriteEffects.FlipHorizontally;

        public bool Hittable { get; set; } = true;
        public int Health { get; set; } = 30;
        public float HitCooldown { get; set; }

        public void Update(LD44Game game, Level level, float delta) {
            PlayerMob playerMob = level.Mobs.OfType<PlayerMob>().First();

            if (!_sighted) {
                if (_sentryTimer <= 0f) {
                    _sentryTimer = 3f + _random.Next(6);
                    if (_random.Next(8) == 0) {
                        _sentryTimer += 5f;
                    }

                    Sprite.Effects = Sprite.Effects == SpriteEffects.None ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                }
                else {
                    _sentryTimer -= delta;
                }
            }

            var sight = new RectangleF(Body.Position.X, Body.Position.Y - 1.5f, 10f, 3f);
            if (!FacingRight) {
                sight.X -= sight.Width;
            }

            RectangleF playerRegion = playerMob.Body.Bounds;
            playerRegion.X = playerMob.Body.Position.X - playerMob.Body.Bounds.Width / 2f;
            playerRegion.Y = playerMob.Body.Position.Y - playerMob.Body.Bounds.Height / 2f;

            float distance = Vector2.Distance(Body.Position, playerMob.Body.Position);
            if (_sighted && distance > 10f) {
                _sighted = false;
            }

            if (!_sighted && (sight.Intersects(playerRegion) || distance < 1.5f)) {
                _sighted = true;

                _state = State.Waiting;
                _stateTimer = 1f;
            }

            if (_sighted) {
                switch (_state) {
                    case State.Waiting: {
                        Body.Velocity = new Vector2(0f, Body.Velocity.Y);
                        if (playerMob.Body.Position.X > Body.Position.X) {
                            Sprite.Effects = SpriteEffects.FlipHorizontally;
                        }
                        else if (playerMob.Body.Position.X < Body.Position.X) {
                            Sprite.Effects = SpriteEffects.None;
                        }

                        _stateTimer -= delta;
                        if (_stateTimer <= 0f) {
                            if (playerMob.Body.Position.Y + 0.5f < Body.Position.Y) {
                                _state = State.Leaping;
                            }
                            else {
                                if (_random.Next(4) == 0) {
                                    _state = State.Leaping;
                                }
                                else {
                                    _state = State.Running;
                                }
                            }

                            if (_state == State.Running) {
                                _stateTimer = 3f;
                            }
                            else {
                                if (playerMob.Body.Position.X > Body.Position.X) {
                                    Body.Velocity = new Vector2(8f, Body.Velocity.Y);
                                }
                                else if (playerMob.Body.Position.X < Body.Position.X) {
                                    Body.Velocity = new Vector2(-8f, Body.Velocity.Y);
                                }
                                Body.Velocity -= new Vector2(0f, 13f);
                                _state = State.Leaping;
                            }
                        }
                        break;
                    }
                    case State.Running: {
                        if (playerMob.Body.Position.X - 0.75f > Body.Position.X) {
                            Body.Velocity = new Vector2(6f, Body.Velocity.Y);
                            Sprite.Effects = SpriteEffects.FlipHorizontally;
                        }
                        else if (playerMob.Body.Position.X + 0.75f < Body.Position.X) {
                            Body.Velocity = new Vector2(-6f, Body.Velocity.Y);
                            Sprite.Effects = SpriteEffects.None;
                        }

                        _stateTimer -= delta;
                        if (_stateTimer <= 0f) {
                            _state = State.Waiting;
                            _stateTimer = 1f;
                        }
                        break;
                    }
                    case State.Leaping: {
                        if (Body.Contact.Y > 0f) {
                            _state = State.Waiting;
                            _stateTimer = 1f;
                        }
                        break;
                    }
                }
            }
        }
    }
}
