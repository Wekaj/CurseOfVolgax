using LD44.Generation;

namespace LD44.Levels {
    public sealed class LevelTemplate {
        public LevelTemplate(ChunkSet chunks, int width, int height, bool sky, string background) {
            Chunks = chunks;
            Width = width;
            Height = height;
            Sky = sky;
            Background = background;
        }

        public ChunkSet Chunks { get; }
        public int Width { get; }
        public int Height { get; }
        public bool Sky { get; }
        public string Background { get; }
    }
}
