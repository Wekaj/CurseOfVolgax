using Ruut.Graphics;

namespace LD44.Levels {
    public enum TileType {
        Air,
        Solid
    }

    public sealed class Tile {
        public TileType TileType { get; set; }

        public Sprite BackSprite { get; } = new Sprite("empty");
        public Sprite FrontSprite { get; } = new Sprite("empty");
    }
}
