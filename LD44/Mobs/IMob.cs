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

        void Update(float delta);
    }
}
