using LD44.Physics;
using Ruut.Graphics;

namespace LD44.Mobs {
    public interface IMob {
        Sprite Sprite { get; }
        Body Body { get; }
    }
}
