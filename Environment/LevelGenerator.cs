using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace game.Environment
{
    public class LevelGenerator
    {
        private Random random = new Random();

        public LevelData GenerateLevel(int width, int height)
        {
            int levelSeed = random.Next(); 
            var level = new LevelData(width, height, levelSeed);

            level.StartRoom = new Point(width / 2, height / 2);
            level.RoomLayout[level.StartRoom.X, level.StartRoom.Y] = RoomType.Start;

            level.EndRoom = FindFarRoom(level, level.StartRoom);
            level.RoomLayout[level.EndRoom.X, level.EndRoom.Y] = RoomType.Boss;

            var path = GeneratePath(level, level.StartRoom, level.EndRoom);
            AddBranchRooms(level, path);
            FillRoomTypes(level);

            return level;
        }

        private Point FindFarRoom(LevelData level, Point start)
        {
            if (start.X < level.Width / 2)
                return new Point(level.Width - 2, level.Height - 2);
            else
                return new Point(1, 1);
        }

        private List<Point> GeneratePath(LevelData level, Point start, Point end)
        {
            var path = new List<Point>();
            var current = start;

            while (current.X != end.X)
            {
                current.X += Math.Sign(end.X - current.X);
                if (level.RoomLayout[current.X, current.Y] == RoomType.None)
                    level.RoomLayout[current.X, current.Y] = RoomType.Normal;
                path.Add(new Point(current.X, current.Y));
            }

            while (current.Y != end.Y)
            {
                current.Y += Math.Sign(end.Y - current.Y);
                if (level.RoomLayout[current.X, current.Y] == RoomType.None)
                    level.RoomLayout[current.X, current.Y] = RoomType.Normal;
                path.Add(new Point(current.X, current.Y));
            }

            return path;
        }

        private void AddBranchRooms(LevelData level, List<Point> mainPath)
        {
            int branchCount = random.Next(3, 7);

            for (int i = 0; i < branchCount; i++)
            {
                var pathRoom = mainPath[random.Next(mainPath.Count)];

                var directions = new[] {
                    new Point(1, 0), new Point(-1, 0),
                    new Point(0, 1), new Point(0, -1)
                };

                foreach (var dir in directions.OrderBy(x => random.Next()))
                {
                    int newX = pathRoom.X + dir.X;
                    int newY = pathRoom.Y + dir.Y;

                    if (newX >= 0 && newX < level.Width &&
                        newY >= 0 && newY < level.Height &&
                        level.RoomLayout[newX, newY] == RoomType.None)
                    {
                        level.RoomLayout[newX, newY] = RoomType.Normal;
                        break;
                    }
                }
            }
        }

        private void FillRoomTypes(LevelData level)
        {
            for (int x = 0; x < level.Width; x++)
            {
                for (int y = 0; y < level.Height; y++)
                {
                    if (level.RoomLayout[x, y] == RoomType.Normal)
                    {
                        int chance = random.Next(100);
                        if (chance < 10)
                            level.RoomLayout[x, y] = RoomType.Shop;
                        else if (chance < 15)
                            level.RoomLayout[x, y] = RoomType.Treasure;
                        else if (chance < 20)
                            level.RoomLayout[x, y] = RoomType.Empty;
                    }
                }
            }
        }
    }
}