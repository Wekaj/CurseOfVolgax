using LD44.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;

namespace LD44.Mobs {
    public sealed class BatMob : IMob {
        private bool _left = true;

        public Sprite Sprite { get; } = new Sprite("bat") {
            Offset = new Vector2(-8f, -8f)
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 0.3f, 0.3f));
        public AnimationState<Sprite> Animation { get; set; }
        public bool Gravity { get; set; } = false;
        public CollisionType CollisionType { get; set; } = CollisionType.Stun;

        public void Update(float delta) {
            if (Body.Contact.X < 0f) {
                _left = false;
            }
            else if (Body.Contact.X > 0f) {
                _left = true;
            }

            if (_left) {
                Sprite.Effects = SpriteEffects.None;
                Body.Velocity = new Vector2(-3f, 0f);
            }
            else {
                Sprite.Effects = SpriteEffects.FlipHorizontally;
                Body.Velocity = new Vector2(3f, 0f);
            }
        }
    }
}
