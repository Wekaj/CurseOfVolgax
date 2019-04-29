using LD44.Items;
using Microsoft.Xna.Framework;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;
using System.Collections.Generic;

namespace LD44.Levels {
    public enum InteractableType {
        Message,
        Door,
        Merchant,
        Blessing,
        Valgox
    }

    public sealed class Interactable {
        public Vector2 Position { get; set; }
        public RectangleF Region { get; set; }
        public Sprite Sprite { get; } = new Sprite("empty");
        public AnimationState<Sprite> Animation { get; set; } = null;

        public InteractableType InteractableType { get; set; }

        public string Message { get; set; }

        public LevelTemplate Destination { get; set; }

        public List<Item> Inventory { get; } = new List<Item>();

        public bool Accepted { get; set; } = false;
    }
}
