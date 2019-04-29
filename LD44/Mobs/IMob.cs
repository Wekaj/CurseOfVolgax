using LD44.Levels;
using LD44.Physics;
using Ruut.Animation;
using Ruut.Graphics;

namespace LD44.Mobs {
    public interface IMob {
        Sprite Sprite { get; }
        Body Body { get; }
        AnimationState<Sprite> Animation { get; set; }
        bool Gravity { get; set; }
        CollisionType CollisionType { get; set; }
        bool Dead { get; set; }

        bool Hittable { get; set; }
        int Health { get; set; }
        float HitCooldown { get; set; }

        void Update(LD44Game game, Level level, float delta);
    }
}
