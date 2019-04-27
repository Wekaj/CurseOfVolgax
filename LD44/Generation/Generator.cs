﻿using LD44.Levels;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace LD44.Generation {
    public static class Generator {
        public static Level GenerateLevel(int width, int height, ChunkSet chunkSet, bool sky, Random random) {
            ChunkSides[,] chunkSides = new ChunkSides[width, height];

            chunkSides[0, 0].Right = SideStatus.Open;
            chunkSides[1, 0].Left = SideStatus.Open;
            chunkSides[0, 0].Bottom = SideStatus.Closed;
            chunkSides[0, 1].Top = SideStatus.Closed;

            chunkSides[width - 1, height - 1].Left = SideStatus.Open;
            chunkSides[width - 2, height - 1].Right = SideStatus.Open;
            chunkSides[width - 1, height - 1].Top = SideStatus.Closed;
            chunkSides[width - 1, height - 2].Bottom = SideStatus.Closed;

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
                            InitializeTile(level, x * chunkSet.Width + x2, y * chunkSet.Height + y2, chunk[x2, y2]);
                        }
                    }
                }
            }

            return level;
        }

        private static void InitializeTile(Level level, int x, int y, ChunkTile tileType) {

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