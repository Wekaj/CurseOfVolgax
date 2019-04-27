﻿using LD44.Physics;
using Microsoft.Xna.Framework;
using Ruut;
using Ruut.Graphics;

namespace LD44.Mobs {
    public sealed class PlayerMob : IMob {
        public Sprite Sprite { get; } = new Sprite("player") {
            Offset = new Vector2(-8f, -11f)
        };

        public Body Body { get; } = new Body(new RectangleF(0f, 0f, 0.25f, 0.6f));
    }
}