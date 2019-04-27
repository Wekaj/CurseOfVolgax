using Microsoft.Xna.Framework;
using Ruut;
using Ruut.Graphics;

namespace LD44.Levels {
    public enum InteractableType {
        Message,
    }

    public sealed class Interactable {
        public Vector2 Position { get; set; }
        public RectangleF Region { get; set; }
        public Sprite Sprite { get; } = new Sprite("empty");

        public InteractableType InteractableType { get; set; }

        public string Message { get; set; }
    }
}
