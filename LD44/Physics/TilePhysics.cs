using LD44.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace LD44.Physics {
    public static class TilePhysics {
        public static void DoTileCollisions(Body body, Level level) {
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

            var overlapping = new List<Point>();
            for (int y = minY; y <= maxY; y++) {
                for (int x = minX; x <= maxX; x++) {
                    if (level.IsWithinBounds(x, y) && IsSolid(level.GetTile(x, y).TileType)) {
                        overlapping.Add(new Point(x, y));
                    }
                }
            }
            overlapping.Sort((p1, p2) => Vector2.DistanceSquared(body.Position, new Vector2(p1.X + 0.5f, p1.Y + 0.5f))
                .CompareTo(Vector2.DistanceSquared(body.Position, new Vector2(p2.X + 0.5f, p2.Y + 0.5f))));

            Vector2 contact = Vector2.Zero;
            foreach (Point tile in overlapping) {
                float leftOverlap = body.Position.X + halfWidth - tile.X;
                float rightOverlap = tile.X + 1f - (body.Position.X - halfWidth);
                float topOverlap = body.Position.Y + halfHeight - tile.Y;
                float bottomOverlap = tile.Y + 1f - (body.Position.Y - halfHeight);

                if (leftOverlap < rightOverlap && leftOverlap < topOverlap && leftOverlap < bottomOverlap) {
                    body.Position = new Vector2(tile.X - halfWidth, body.Position.Y);
                    body.Velocity = new Vector2(Math.Min(body.Velocity.X, 0f), body.Velocity.Y);
                    contact.X++;
                }
                else if (rightOverlap < topOverlap && rightOverlap < bottomOverlap) {
                    body.Position = new Vector2(tile.X + 1f + halfWidth, body.Position.Y);
                    body.Velocity = new Vector2(Math.Max(body.Velocity.X, 0f), body.Velocity.Y);
                    contact.X--;
                }
                else if (topOverlap < bottomOverlap) {
                    body.Position = new Vector2(body.Position.X, tile.Y - halfHeight);
                    body.Velocity = new Vector2(body.Velocity.X, Math.Min(body.Velocity.Y, 0f));
                    contact.Y++;
                }
                else {
                    body.Position = new Vector2(body.Position.X, tile.Y + 1f + halfHeight);
                    body.Velocity = new Vector2(body.Velocity.X, Math.Max(body.Velocity.Y, 0f));
                    contact.Y--;
                }
            }
            body.Contact = contact;
        }

        public static bool IsSolid(TileType tileType) {
            switch (tileType) {
                case TileType.Rock: {
                    return true;
                }
                default: {
                    return false;
                }
            }
        }
    }
}
