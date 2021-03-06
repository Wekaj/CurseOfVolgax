﻿using LD44.Levels;
using Microsoft.Xna.Framework;
using Ruut;
using System;
using System.Collections.Generic;

namespace LD44.Physics {
    public static class TilePhysics {
        public static void DoTileCollisions(Body body, Level level) {
            if (body.Ghost) {
                return;
            }

            float halfWidth = body.Bounds.Width / 2f;
            float halfHeight = body.Bounds.Height / 2f;

            float left = body.Position.X - halfWidth;
            float right = body.Position.X + halfWidth;
            float top = body.Position.Y - halfHeight;
            float bottom = body.Position.Y + halfHeight;

            int minX = (int)Math.Floor(left);
            int maxX = (int)Math.Floor(right);
            int minY = (int)Math.Floor(top);
            int maxY = (int)Math.Floor(bottom);

            var overlapping = new List<RectangleF>();
            RectangleF? last = null;
            for (int y = minY; y <= maxY; y++) {
                last = null;

                for (int x = minX; x <= maxX; x++) {
                    if (level.IsWithinBounds(x, y) && IsSolid(level.GetTile(x, y).TileType)) {
                        if (last != null) {
                            RectangleF rect = last.Value;
                            rect.Width++;
                            last = rect;
                        }
                        else {
                            last = new RectangleF(x, y, 1, 1);
                        }
                    }
                    else {
                        if (last != null) {
                            overlapping.Add(last.Value);
                        }
                        last = null;
                    }
                }

                if (last != null) {
                    overlapping.Add(last.Value);
                }
            }
            overlapping.Sort((r1, r2) => Vector2.DistanceSquared(body.Position, new Vector2(r1.X + r1.Width / 2f, r1.Y + r1.Height / 2f))
                .CompareTo(Vector2.DistanceSquared(body.Position, new Vector2(r2.X + r2.Width / 2f, r2.Y + r2.Height / 2f))));

            Vector2 contact = Vector2.Zero;
            foreach (RectangleF tile in overlapping) {
                float leftOverlap = body.Position.X + halfWidth - tile.X;
                float rightOverlap = tile.X + tile.Width - (body.Position.X - halfWidth);
                float topOverlap = body.Position.Y + halfHeight - tile.Y;
                float bottomOverlap = tile.Y + tile.Height - (body.Position.Y - halfHeight);

                if (leftOverlap < rightOverlap && leftOverlap < topOverlap && leftOverlap < bottomOverlap) {
                    body.Position = new Vector2(tile.X - halfWidth, body.Position.Y);
                    if (!body.Bouncy) {
                        body.Velocity = new Vector2(Math.Min(body.Velocity.X, 0f), body.Velocity.Y);
                    }
                    else {
                        body.Velocity = new Vector2(-body.Velocity.X * body.BounceFactor, body.Velocity.Y);
                    }
                    contact.X++;
                }
                else if (rightOverlap < topOverlap && rightOverlap < bottomOverlap) {
                    body.Position = new Vector2(tile.X + tile.Width + halfWidth, body.Position.Y);
                    if (!body.Bouncy) {
                        body.Velocity = new Vector2(Math.Max(body.Velocity.X, 0f), body.Velocity.Y);
                    }
                    else {
                        body.Velocity = new Vector2(-body.Velocity.X * body.BounceFactor, body.Velocity.Y);
                    }
                    contact.X--;
                }
                else if (topOverlap < bottomOverlap) {
                    body.Position = new Vector2(body.Position.X, tile.Y - halfHeight);
                    if (!body.Bouncy) {
                        body.Velocity = new Vector2(body.Velocity.X, Math.Min(body.Velocity.Y, 0f));
                    }
                    else {
                        body.Velocity = new Vector2(body.Velocity.Y, -body.Velocity.Y * body.BounceFactor);
                    }
                    contact.Y++;
                }
                else {
                    body.Position = new Vector2(body.Position.X, tile.Y + tile.Height + halfHeight);
                    if (!body.Bouncy) {
                        body.Velocity = new Vector2(body.Velocity.X, Math.Max(body.Velocity.Y, 0f));
                    }
                    else {
                        body.Velocity = new Vector2(body.Velocity.Y, -body.Velocity.Y * body.BounceFactor);
                    }
                    contact.Y--;
                }
            }
            body.Contact = contact;
        }

        public static bool IsSolid(TileType tileType) {
            switch (tileType) {
                case TileType.Solid: {
                    return true;
                }
                default: {
                    return false;
                }
            }
        }
    }
}
