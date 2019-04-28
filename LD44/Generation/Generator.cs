﻿using LD44.Levels;
using LD44.Mobs;
using Microsoft.Xna.Framework;
using Ruut;
using Ruut.Animation;
using Ruut.Graphics;
using System;
using System.Collections.Generic;

namespace LD44.Generation {
    public static class Generator {
        public static Level GenerateLevel(LD44Game game, LevelTemplate template, Random random) {
            return GenerateLevel(game, template.Width, template.Height, template.Chunks, template.Sky, template.Background, random);
        }

        public static Level GenerateLevel(LD44Game game, int width, int height, ChunkSet chunkSet, bool sky, string background, Random random) {
            ChunkSides[,] chunkSides = new ChunkSides[width, height];

            if (width > 1) {
                chunkSides[0, 0].Right = SideStatus.Open;
                chunkSides[1, 0].Left = SideStatus.Open;
            }
            if (height > 1) {
                chunkSides[0, 0].Bottom = SideStatus.Closed;
                chunkSides[0, 1].Top = SideStatus.Closed;
            }

            if (width > 1) {
                chunkSides[width - 1, height - 1].Left = SideStatus.Open;
                chunkSides[width - 2, height - 1].Right = SideStatus.Open;
            }
            if (height > 1) {
                chunkSides[width - 1, height - 1].Top = SideStatus.Closed;
                chunkSides[width - 1, height - 2].Bottom = SideStatus.Closed;
            }

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    if (x == 0) {
                        chunkSides[x, y].Left = SideStatus.Closed;
                    }
                    if (y == 0) {
                        chunkSides[x, y].Top = sky ? SideStatus.Edge : SideStatus.Closed;
                    }
                    if (x == width - 1) {
                        chunkSides[x, y].Right = SideStatus.Closed;
                    }
                    if (y == height - 1) {
                        chunkSides[x, y].Bottom = SideStatus.Closed;
                    }
                }
            }

            Tunnel(chunkSides, 1, 0, random);

            while (TunnelUncertainChunk(chunkSides, random));

            var level = new Level(chunkSet.Width * width, chunkSet.Height * height);

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    Chunk chunk = chunkSet.Get(chunkSides[x, y], random);

                    for (int y2 = 0; y2 < chunkSet.Height; y2++) {
                        for (int x2 = 0; x2 < chunkSet.Width; x2++) {
                            InitializeTile(game, level, x * chunkSet.Width + x2, y * chunkSet.Height + y2, chunk[x2, y2]);
                        }
                    }
                }
            }

            int fullHeight = height * chunkSet.Height;
            int fullWidth = width * chunkSet.Width;
            for (int y = 0; y < fullHeight; y++) {
                for (int x = 0; x < fullWidth; x++) {
                    Tile tile = level.GetTile(x, y);
                    
                    if (tile.TileType != TileType.Solid) {
                        continue;
                    }

                    if (y == 0 || level.GetTile(x, y - 1).TileType == TileType.Solid) {
                        if (y == fullHeight - 1 || level.GetTile(x, y + 1).TileType == TileType.Solid) {
                            tile.FrontSprite.Texture += "_center";
                        }
                        else {
                            tile.FrontSprite.Texture += "_bottom";
                        }
                    }
                    else {
                        tile.FrontSprite.Texture += "_top";
                    }
                }
            }

            level.Background.Texture = background;

            return level;
        }

        private static void InitializeTile(LD44Game game, Level level, int x, int y, ChunkTile tileType) {
            Tile tile = level.GetTile(x, y);

            switch (tileType) {
                case ChunkTile.Rock: {
                    tile.FrontSprite.Texture = "rock";
                    tile.TileType = TileType.Solid;
                    break;
                }
                case ChunkTile.RockWall: {
                    tile.BackSprite.Texture = "rock_wall";
                    break;
                }
                case ChunkTile.Sapling: {
                    tile.BackSprite.Texture = "sapling";
                    break;
                }
                case ChunkTile.Bat: {
                    var bat = new BatMob {
                        Animation = new AnimationState<Sprite>(game.SpriteAnimations["bat_flying"], 0.5f) {
                            IsLooping = true
                        }
                    };
                    bat.Body.Position = new Vector2(x, y) + new Vector2(0.5f);
                    level.Mobs.Add(bat);
                    break;
                }
                case ChunkTile.Player: {
                    level.Entrance = new Vector2(x + 0.5f, y + 0.5f);
                    break;
                }
                case ChunkTile.Door: {
                    tile.BackSprite.Texture = "rock_wall";

                    var door = new Interactable {
                        Position = new Vector2(x, y) + new Vector2(0.5f),
                        Region = new RectangleF(0f, 0f, 1f, 1f),

                        InteractableType = InteractableType.Door,

                        Destination = game.JungleTemplate
                    };
                    door.Sprite.Texture = "door";
                    door.Sprite.Origin = new Vector2(0.5f);
                    level.Interactables.Add(door);
                    break;
                }
                case ChunkTile.Greeter: {
                    var talker = new Interactable {
                        Position = new Vector2(x, y) + new Vector2(0.5f),
                        Region = new RectangleF(0f, 0f, 1f, 1f),

                        InteractableType = InteractableType.Message,

                        Message = "Hohohohoho, you dare enter the domain of Valgox uninvited? You fool!"
                    };
                    talker.Animation = new AnimationState<Sprite>(game.SpriteAnimations["trader_idle"], 0.5f) {
                        IsLooping = true
                    };
                    talker.Sprite.Texture = "trader";
                    talker.Sprite.Origin = new Vector2(0.5f);
                    level.Interactables.Add(talker);
                    break;
                }
            }
        }

        private static void Tunnel(ChunkSides[,] chunkSides, int x, int y, Random random) {
            ChunkSides current = chunkSides[x, y];

            var choices = new List<Point>();
            if (current.Left == SideStatus.Undecided) {
                choices.Add(new Point(-1, 0));
                current.Left = SideStatus.Closed;
            }
            if (current.Right == SideStatus.Undecided) {
                choices.Add(new Point(1, 0));
                current.Right = SideStatus.Closed;
            }
            if (current.Top == SideStatus.Undecided) {
                choices.Add(new Point(0, -1));
                current.Top = SideStatus.Closed;
            }
            if (current.Bottom == SideStatus.Undecided) {
                choices.Add(new Point(0, 1));
                current.Bottom = SideStatus.Closed;
            }

            if (choices.Count == 0) {
                return;
            }

            Point choice = choices[random.Next(choices.Count)];
            ChunkSides choiceSides = chunkSides[x + choice.X, y + choice.Y];
            if (choice.X < 0) {
                current.Left = SideStatus.Open;
                choiceSides.Right = SideStatus.Open;
            }
            if (choice.X > 0) {
                current.Right = SideStatus.Open;
                choiceSides.Left = SideStatus.Open;
            }
            if (choice.Y < 0) {
                current.Top = SideStatus.Open;
                choiceSides.Bottom = SideStatus.Open;
            }
            if (choice.Y > 0) {
                current.Bottom = SideStatus.Open;
                choiceSides.Top = SideStatus.Open;
            }

            chunkSides[x, y] = current;
            chunkSides[x + choice.X, y + choice.Y] = choiceSides; 

            Tunnel(chunkSides, x + choice.X, y + choice.Y, random);
        }

        private static bool TunnelUncertainChunk(ChunkSides[,] chunkSides, Random random) {
            int width = chunkSides.GetLength(0);
            int height = chunkSides.GetLength(1);

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    ChunkSides sides = chunkSides[x, y];

                    if (sides.Left == SideStatus.Undecided || sides.Right == SideStatus.Undecided
                        || sides.Top == SideStatus.Undecided || sides.Bottom == SideStatus.Undecided) {
                        Tunnel(chunkSides, x, y, random);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
