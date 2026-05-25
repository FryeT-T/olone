using System;
using System.Drawing;

namespace game.Environment
{
    public enum RoomType
    {
        None = 0,
        Normal = 1,
        Start = 2,
        Shop = 3,
        Treasure = 4,
        Boss = 5,
        Empty = 6
    }

    public class LevelData
    {
        public string LevelName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Point StartRoom { get; set; }
        public Point EndRoom { get; set; }
        public RoomType[,] RoomLayout { get; set; }
        public int Seed { get; set; }

        public LevelData(int width, int height, int seed) 
        {
            Width = width;
            Height = height;
            Seed = seed;
            RoomLayout = new RoomType[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    RoomLayout[x, y] = RoomType.None;
        }

        public bool HasRoom(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return false;
            return RoomLayout[x, y] != RoomType.None;
        }

        public RoomType GetRoomType(int x, int y)
        {
            if (!HasRoom(x, y))
                return RoomType.None;
            return RoomLayout[x, y];
        }
    }
}