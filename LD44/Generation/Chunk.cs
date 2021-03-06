﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LD44.Generation {
    public enum ChunkTile {
        Air,
        Rock,
        Bat,
        Player,
        Door,
        Greeter,
        RockWall,
        Sapling,
        Leaf,
        Spikes,
        Spot,
        Monkey,
        EliteBat,
        Gem,
        Marble,
        Angel,
        Waterfall,
        Roof,
        HouseWall,
        HouseWindow,
        Villager,
        Entrance,
        Exit,
        Valgox,
        Curse
    }

    public sealed class Chunk {
        private readonly ChunkTile[,] _tiles;

        public Chunk() {
        }

        public Chunk(int width, int height) {
            _tiles = new ChunkTile[width, height];
        }

        public ChunkTile this[int x, int y] => _tiles[x, y];

        public int Width => _tiles.GetLength(0);
        public int Height => _tiles.GetLength(1);

        public int Rarity { get; private set; }

        public ChunkSides Sides { get; private set; } = new ChunkSides();

        public static Chunk FromTexture(Texture2D texture, Rectangle region) {
            var chunk = new Chunk(region.Width - 1, region.Height);

            var texturePixels = new Color[texture.Width * texture.Height];
            texture.GetData(texturePixels);

            var pixels = new Color[region.Width, region.Height];
            for (int y = region.Top; y < region.Bottom; y++) {
                for (int x = region.Left; x < region.Right; x++) {
                    pixels[x - region.Left, y - region.Top] = texturePixels[x + y * texture.Width];
                }
            }

            for (int y = 0; y < region.Height; y++) {
                for (int x = 0; x < region.Width - 1; x++) {
                    chunk._tiles[x, y] = GetChunkTile(pixels[x, y]);
                }
            }

            chunk.Rarity = pixels[region.Width - 1, 0].A;

            chunk.Sides = new ChunkSides {
                Left = GetSideStatus(pixels[region.Width - 1, 1]),
                Right = GetSideStatus(pixels[region.Width - 1, 2]),
                Top = GetSideStatus(pixels[region.Width - 1, 3]),
                Bottom = GetSideStatus(pixels[region.Width - 1, 4])
            };

            return chunk;
        }

        private static ChunkTile GetChunkTile(Color color) {
            if (color == Color.Black) {
                return ChunkTile.Rock;
            }
            else if (Matches(color, 255, 0, 0)) {
                return ChunkTile.Bat;
            }
            else if (Matches(color, 0, 255, 0)) {
                return ChunkTile.Player;
            }
            else if (Matches(color, 0, 0, 255)) {
                return ChunkTile.Door;
            }
            else if (Matches(color, 255, 255, 0)) {
                return ChunkTile.Greeter;
            }
            else if (Matches(color, 100, 0, 0)) {
                return ChunkTile.RockWall;
            }
            else if (Matches(color, 0, 0, 100)) {
                return ChunkTile.Sapling;
            }
            else if (Matches(color, 50, 50, 50)) {
                return ChunkTile.Leaf;
            }
            else if (Matches(color, 0, 100, 0)) {
                return ChunkTile.Spikes;
            }
            else if (Matches(color, 0, 255, 255)) {
                return ChunkTile.Spot;
            }
            else if (Matches(color, 0, 0, 150)) {
                return ChunkTile.Monkey;
            }
            else if (Matches(color, 150, 0, 0)) {
                return ChunkTile.EliteBat;
            }
            else if (Matches(color, 0, 150, 0)) {
                return ChunkTile.Gem;
            }
            else if (Matches(color, 100, 100, 100)) {
                return ChunkTile.Marble;
            }
            else if (Matches(color, 200, 0, 0)) {
                return ChunkTile.Angel;
            }
            else if (Matches(color, 0, 0, 200)) {
                return ChunkTile.Waterfall;
            }
            else if (Matches(color, 100, 100, 0)) {
                return ChunkTile.Roof;
            }
            else if (Matches(color, 0, 100, 100)) {
                return ChunkTile.HouseWall;
            }
            else if (Matches(color, 100, 0, 100)) {
                return ChunkTile.HouseWindow;
            }
            else if (Matches(color, 150, 100, 100)) {
                return ChunkTile.Villager;
            }
            else if (Matches(color, 150, 100, 150)) {
                return ChunkTile.Entrance;
            }
            else if (Matches(color, 150, 150, 100)) {
                return ChunkTile.Exit;
            }
            else if (Matches(color, 255, 100, 255)) {
                return ChunkTile.Valgox;
            }
            else if (Matches(color, 50, 200, 100)) {
                return ChunkTile.Curse;
            }
            return ChunkTile.Air;
        }

        private static SideStatus GetSideStatus(Color color) {
            if (Matches(color, 0, 255, 0)) {
                return SideStatus.Open;
            }
            if (Matches(color, 255, 0, 0)) {
                return SideStatus.Closed;
            }
            return SideStatus.Edge;
        }

        private static bool Matches(Color color, byte r, byte g, byte b, byte a = 255) {
            return color.R == r && color.G == g && color.B == b && color.A == a;
        }
    }
}
