using LD44.Levels;
using LD44.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;
using System.Linq;

namespace LD44.Mobs {
    public sealed class VillagerMob : IMob {
        private float _shootCharger = 3f;

        public Sprite Sprite { get; } = new Sprite("villager") {
            Offset = new Vector2(-8f, -8f)
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 0.75f, 0.75f));
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = false;
        public CollisionType CollisionType { get; set; } = CollisionType.Stun;
        public bool Dead { get; set; }

        public bool Hittable { get; set; } = true;
        public int Health { get; set; } = 40;
        public float HitCooldown { get; set; }

        public void Update(LD44Game game, Level level, float delta) {
            PlayerMob playerMob = level.Mobs.OfType<PlayerMob>().First();

            if (Vector2.Distance(playerMob.Body.Position, Body.Position) < 7f) {
                if (playerMob.Body.Position.X > Body.Position.X) {
                    Sprite.Effects = SpriteEffects.FlipHorizontally;
                }
                else if (playerMob.Body.Position.X < Body.Position.X) {
                    Sprite.Effects = SpriteEffects.None;
                }

                _shootCharger -= delta;
                if (_shootCharger <= 0f) {
                    game.Content.Load<SoundEffect>("Sounds/projectile").Play();

                    var projectile = new ProjectileMob {
                        Animation = new AnimationState<Sprite>(game.SpriteAnimations["projectile"], 0.2f) {
                            IsLooping = true
                        }
                    };
                    projectile.Body.Position = Body.Position;
                    projectile.Body.Velocity = Vector2.Normalize(playerMob.Body.Position - Body.Position) * 20f;
                    level.FutureMobs.Add(projectile);

                    _shootCharger = 3f;
                }
            }
        }
    }
}
