using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LD44.Generation {
    public sealed class ChunkSet {
        private readonly Dictionary<ChunkSides, List<Chunk>> _chunks = new Dictionary<ChunkSides, List<Chunk>>();

        private int? _width, _height;

        public int Width => _width.Value;
        public int Height => _height.Value;

        public void Add(Chunk chunk) {
            if (_width == null) {
                _width = chunk.Width;
                _height = chunk.Height;
            }
            else if (chunk.Width != Width || chunk.Height != Height) {
                throw new ArgumentException("Chunk dimensions must match.");
            }

            if (_chunks.TryGetValue(chunk.Sides, out List<Chunk> list)) {
                list.Add(chunk);
            }
            else {
                _chunks.Add(chunk.Sides, new List<Chunk> { chunk });
            }
        }

        public Chunk Get(ChunkSides match, Random random) {
            List<Chunk> list = _chunks[match];

            int value = random.Next(list.Sum(c => c.Rarity));

            int chunk = 0;
            if (value >= list[chunk].Rarity) {
                value -= list[chunk].Rarity;
                chunk++;
            }

            return list[chunk];
        }

        public static ChunkSet FromTexture(Texture2D texture, int count) {
            var set = new ChunkSet();

            int width = texture.Width / count;

            var region = new Rectangle(0, 0, width, texture.Height);
            for (int i = 0; i < count; i++) {
                set.Add(Chunk.FromTexture(texture, region));
                region.X += width;
            }

            return set;
        }
    }
}
