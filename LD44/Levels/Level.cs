using LD44.Mobs;
using Microsoft.Xna.Framework;
using Ruut.Graphics;
using System.Collections.Generic;

namespace LD44.Levels {
    public sealed class Level {
        private readonly Tile[,] _tiles;

        public Level(int width, int height) {
            _tiles = new Tile[width, height];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    _tiles[x, y] = new Tile();
                }
            }
        }

        public int Width => _tiles.GetLength(0);
        public int Height => _tiles.GetLength(1);

        public IList<IMob> Mobs { get; } = new List<IMob>();
        public IList<IMob> FutureMobs { get; } = new List<IMob>();
        public IList<Interactable> Interactables { get; } = new List<Interactable>();

        public Vector2? Entrance { get; set; }

        public Sprite Background { get; } = new Sprite("empty");

        public IList<Point> Entrances { get; } = new List<Point>();
        public IList<Point> Exits { get; } = new List<Point>();
        public IList<Point> Spots { get; } = new List<Point>();

        public Tile GetTile(int x, int y) {
            return _tiles[x, y];
        }

        public bool IsWithinBounds(int x, int y) {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }
    }
}
