using LD44.Levels;
using LD44.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;
using System;
using System.Linq;

namespace LD44.Mobs {
    public sealed class ValgoxMob : IMob {
        private enum State {
            None,
            PreparingToChargeLeft,
            ChargingLeft,
            ChargingRight,
            PreparingToChargeRight,
            Summoning,
            PreparingToShoot,
            Shooting
        }

        private readonly Random _random = new Random();

        private State _state = State.None;

        private bool _hasTarget = false;
        private Vector2 _startPosition, _targetPosition;
        private float _timer;
        private float _speed;
        private float _arc = 2f;
        private float _smoothing = 3f;

        private int _chargeNum;

        private int _shotNum;
        private float _shootTimer = 0f;

        private float _thinkTimer = 1f;

        private int _lastAction = -1;

        public Sprite Sprite { get; } = new Sprite("valgox") {
            Offset = new Vector2(-16f, -16f)
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 1f, 1.25f)) {
            Ghost = true
        };
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = false;
        public CollisionType CollisionType { get; set; } = CollisionType.StunPlus;
        public bool Dead { get; set; }

        public bool FacingRight => Sprite.Effects == SpriteEffects.FlipHorizontally;

        public bool Hittable { get; set; } = true;
        public int Health { get; set; } = 60;
        public float HitCooldown { get; set; }

        public void Update(LD44Game game, Level level, float delta) {
            if (_hasTarget) {
                _timer += delta;
                if (_timer > _speed) {
                    _timer = _speed;
                }

                float p = _timer / _speed;
                p = 1f - (float)Math.Pow(1f - p, _smoothing);

                Body.Position = _startPosition * (1f - p) + _targetPosition * p - new Vector2(0f, _arc * (float)Math.Sin(p * (float)Math.PI));
            }

            switch (_state) {
                case State.None: {
                    _thinkTimer -= delta;

                    if (_thinkTimer <= 0f) {
                        int action = _random.Next(3);

                        if (action == _lastAction) {
                            action--;
                            if (action < 0) {
                                action = 2;
                            }
                        }

                        _lastAction = action;

                        switch (action) {
                            case 0: {
                                if (_random.Next(2) == 0) {
                                    _state = State.PreparingToChargeLeft;

                                    _hasTarget = true;
                                    _startPosition = Body.Position;
                                    _targetPosition = new Vector2(25.5f, 15.5f);
                                    _timer = 0f;
                                    _speed = 2f;
                                    _arc = 2f;
                                    _smoothing = 3f;

                                    _chargeNum = 0;
                                }
                                else {
                                    _state = State.PreparingToChargeRight;

                                    _hasTarget = true;
                                    _startPosition = Body.Position;
                                    _targetPosition = new Vector2(6.5f, 15.5f);
                                    _timer = 0f;
                                    _speed = 2f;
                                    _arc = 2f;
                                    _smoothing = 3f;

                                    _chargeNum = 0;
                                    break;
                                }
                                break;
                            }
                            case 1: {
                                _state = State.Summoning;

                                _hasTarget = true;
                                _startPosition = Body.Position;
                                _targetPosition = new Vector2(12f + 8f * (float)_random.NextDouble(), 13.5f);
                                _timer = 0f;
                                _speed = 2f;
                                _arc = 1f;
                                _smoothing = 2f;
                                break;
                            }
                            case 2: {
                                _state = State.PreparingToShoot;

                                _hasTarget = true;
                                _startPosition = Body.Position;
                                _targetPosition = new Vector2(12f + 8f * (float)_random.NextDouble(), 9.5f);
                                _timer = 0f;
                                _speed = 2f;
                                _arc = 2f;
                                _smoothing = 3f;
                                break;
                            }
                        }
                    }
                    break;
                }
                case State.PreparingToChargeLeft: {
                    if (_timer >= _speed) {
                        _startPosition = Body.Position;
                        _targetPosition = new Vector2(5.5f, 15.5f);
                        _timer = 0f;
                        _speed = 1.75f - _chargeNum / 7f;
                        _state = State.ChargingLeft;
                        _arc = 0f;
                        _smoothing = 1.5f;

                        game.Content.Load<SoundEffect>("Sounds/dash").Play();
                    }
                    break;
                }
                case State.PreparingToChargeRight: {
                    if (_timer >= _speed) {
                        _startPosition = Body.Position;
                        _targetPosition = new Vector2(26.5f, 15.5f);
                        _timer = 0f;
                        _speed = 1.75f - _chargeNum / 7f;
                        _state = State.ChargingRight;
                        _arc = 0f;
                        _smoothing = 1.5f;

                        game.Content.Load<SoundEffect>("Sounds/dash").Play();
                    }
                    break;
                }
                case State.ChargingLeft: {
                    if (_timer >= _speed) {
                        _chargeNum++;

                        if (_chargeNum < 5) {
                            if (_random.Next(3) == 0) {
                                _startPosition = Body.Position;
                                _targetPosition = new Vector2(26.5f, 15.5f);
                                _timer = 0f;
                                _speed = 1.75f - _chargeNum / 7f;
                                _state = State.ChargingRight;
                                _arc = 0f;
                                _smoothing = 1.5f;

                                game.Content.Load<SoundEffect>("Sounds/dash").Play();
                            }
                            else {
                                _state = State.PreparingToChargeLeft;

                                _hasTarget = true;
                                _startPosition = Body.Position;
                                _targetPosition = new Vector2(25.5f, 15.5f);
                                _timer = 0f;
                                _speed = 2f - _chargeNum / 5f;
                                _arc = 2f;
                                _smoothing = 3f - _chargeNum / 5f;
                            }
                        }
                        else {
                            _state = State.None;
                            _thinkTimer = 2f;
                        }
                    }
                    break;
                }
                case State.ChargingRight: {
                    if (_timer >= _speed) {
                        _chargeNum++;

                        if (_chargeNum < 5) {
                            if (_random.Next(3) == 0) {
                                _startPosition = Body.Position;
                                _targetPosition = new Vector2(5.5f, 15.5f);
                                _timer = 0f;
                                _speed = 1.75f - _chargeNum / 7f;
                                _state = State.ChargingLeft;
                                _arc = 0f;
                                _smoothing = 1.5f;

                                game.Content.Load<SoundEffect>("Sounds/dash").Play();
                            }
                            else {
                                _state = State.PreparingToChargeRight;

                                _hasTarget = true;
                                _startPosition = Body.Position;
                                _targetPosition = new Vector2(6.5f, 15.5f);
                                _timer = 0f;
                                _speed = 2f - _chargeNum / 5f;
                                _arc = 2f;
                                _smoothing = 3f - _chargeNum / 5f;
                            }
                        }
                        else {
                            _state = State.None;
                            _thinkTimer = 2f;
                        }
                    }
                    break;
                }
                case State.Summoning: {
                    if (_timer > _speed * 3f / 4f) {
                        Animation = new AnimationState<Sprite>(game.SpriteAnimations["valgox_aiming"], 1f);
                    }

                    if (_timer >= _speed) {
                        var bat = new EliteBatMob {
                            Animation = new AnimationState<Sprite>(game.SpriteAnimations["elite_bat_flying"], 0.5f) {
                                IsLooping = true
                            }
                        };
                        bat.Body.Position = new Vector2(6.5f + 19f * (float)_random.NextDouble(), 12f);
                        level.FutureMobs.Add(bat);

                        _state = State.None;
                        _thinkTimer = 2f;

                        Animation = new AnimationState<Sprite>(game.SpriteAnimations["valgox_idle"], 0.5f) {
                            IsLooping = true
                        };
                    }
                    break;
                }
                case State.PreparingToShoot: {
                    if (_timer >= _speed) {
                        _state = State.Shooting;
                        _shootTimer = 0.5f;
                        _shotNum = 0;
                    }
                    break;
                }
                case State.Shooting: {
                    _shootTimer -= delta;

                    if (_shootTimer < 0.2f) {
                        Animation = new AnimationState<Sprite>(game.SpriteAnimations["valgox_aiming"], 1f);
                    }
                    else if (Animation.Animation != game.SpriteAnimations["valgox_idle"]) {
                        Animation = new AnimationState<Sprite>(game.SpriteAnimations["valgox_idle"], 0.5f) {
                            IsLooping = true
                        };
                    }

                    if (_shootTimer <= 0f) {
                        game.Content.Load<SoundEffect>("Sounds/projectile").Play();

                        PlayerMob playerMob = level.Mobs.OfType<PlayerMob>().First();

                        var projectile = new ProjectileMob {
                            Animation = new AnimationState<Sprite>(game.SpriteAnimations["projectile"], 0.2f) {
                                IsLooping = true
                            }
                        };
                        projectile.Body.Position = Body.Position;
                        projectile.Body.Velocity = Vector2.Normalize(playerMob.Body.Position - Body.Position) * 20f;
                        level.FutureMobs.Add(projectile);

                        _shotNum++;
                        _shootTimer += 2f / _shotNum;

                        _hasTarget = true;
                        _startPosition = Body.Position;
                        _targetPosition = new Vector2(12f + 8f * (float)_random.NextDouble(), 9f + 3f * (float)_random.NextDouble());
                        _timer = 0f;
                        _speed = _shootTimer;
                        _arc = 0f;
                        _smoothing = 2f;
                    }

                    if (_shotNum >= 8) {
                        _state = State.None;
                        _thinkTimer = 2f;

                        Animation = new AnimationState<Sprite>(game.SpriteAnimations["valgox_idle"], 0.5f) {
                            IsLooping = true
                        };
                    }
                    break;
                }
            }
        }
    }
}
