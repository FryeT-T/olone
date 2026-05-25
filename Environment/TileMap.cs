using System;
using System.Drawing;

namespace game.Environment
{
    public enum TileType
    {
        Empty = 0,
        Wall = 1,
        DoorOpened = 3,
        DoorClosed = 4,
    }

    public class TileMap
    {
        public TileType[,] tiles;
        public int Width, Height;
        public int TileSize = 40;

        public int DoorTopIndex { get; private set; } = -1;
        public int DoorBottomIndex { get; private set; } = -1;
        public int DoorLeftIndex { get; private set; } = -1;
        public int DoorRightIndex { get; private set; } = -1;

        public TileMap(int width, int height)
        {
            Width = width;
            Height = height;
            tiles = new TileType[width, height];
        }

        public void GenerateRoom()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    tiles[x, y] = TileType.Wall;
                }
            }

            for (int x = 2; x < Width - 2; x++)
            {
                for (int y = 2; y < Height - 2; y++)
                {
                    tiles[x, y] = TileType.Empty;
                }
            }
        }

        public void SetDoorPosition(int doorIndex, Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    DoorTopIndex = doorIndex;
                    break;
                case Direction.Down:
                    DoorBottomIndex = doorIndex;
                    break;
                case Direction.Left:
                    DoorLeftIndex = doorIndex;
                    break;
                case Direction.Right:
                    DoorRightIndex = doorIndex;
                    break;
            }
        }

        public RectangleF GetTileBounds(int tileX, int tileY)
        {
            return new RectangleF(tileX * TileSize, tileY * TileSize, TileSize, TileSize);
        }

        public bool IsWalkable(int tileX, int tileY)
        {
            if (tileX < 0 || tileX >= Width || tileY < 0 || tileY >= Height)
                return false;

            return tiles[tileX, tileY] == TileType.Empty ||
                   tiles[tileX, tileY] == TileType.DoorOpened;
        }

        public bool CheckCollisionWithTileMap(RectangleF entityBounds)
        {
            float eps = 0.01f;

            int leftTile = (int)((entityBounds.Left + eps) / TileSize);
            int rightTile = (int)((entityBounds.Right - eps) / TileSize);
            int topTile = (int)((entityBounds.Top + eps) / TileSize);
            int bottomTile = (int)((entityBounds.Bottom - eps) / TileSize);

            leftTile = Math.Max(0, leftTile);
            rightTile = Math.Min(Width - 1, rightTile);
            topTile = Math.Max(0, topTile);
            bottomTile = Math.Min(Height - 1, bottomTile);

            for (int y = topTile; y <= bottomTile; y++)
            {
                for (int x = leftTile; x <= rightTile; x++)
                {
                    if (!IsWalkable(x, y))
                        return true;
                }
            }
            return false;
        }

        public bool CanMoveTo(RectangleF entityBounds, float moveX, float moveY)
        {
            RectangleF newBounds = new RectangleF(
                entityBounds.X + moveX,
                entityBounds.Y + moveY,
                entityBounds.Width,
                entityBounds.Height
            );
            return !CheckCollisionWithTileMap(newBounds);
        }

        public bool IsOnDoorTransition(RectangleF entityBounds, out Direction doorDirection)
        {
            doorDirection = Direction.None;

            float centerX = entityBounds.X + entityBounds.Width / 2;
            float centerY = entityBounds.Y + entityBounds.Height / 2;
            int centerTileX = (int)(centerX / TileSize);
            int centerTileY = (int)(centerY / TileSize);

            if (DoorTopIndex >= 0 && centerTileY == 0 &&
                Math.Abs(centerTileX - DoorTopIndex) <= 1)
            {
                doorDirection = Direction.Up;
                return true;
            }

            if (DoorBottomIndex >= 0 && centerTileY == Height - 1 &&
                Math.Abs(centerTileX - DoorBottomIndex) <= 1)
            {
                doorDirection = Direction.Down;
                return true;
            }

            if (DoorLeftIndex >= 0 && centerTileX == 0 &&
                Math.Abs(centerTileY - DoorLeftIndex) <= 1)
            {
                doorDirection = Direction.Left;
                return true;
            }

            if (DoorRightIndex >= 0 && centerTileX == Width - 1 &&
                Math.Abs(centerTileY - DoorRightIndex) <= 1)
            {
                doorDirection = Direction.Right;
                return true;
            }

            return false;
        }

        public void Draw(Graphics g)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Rectangle rect = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);

                    switch (tiles[x, y])
                    {
                        case TileType.Wall:
                            using (SolidBrush brush = new SolidBrush(Color.FromArgb(80, 80, 80)))
                            {
                                g.FillRectangle(brush, rect);
                            }
                            using (Pen pen = new Pen(Color.FromArgb(60, 60, 60), 1))
                            {
                                g.DrawRectangle(pen, rect);
                            }
                            break;

                        case TileType.DoorOpened:
                            using (SolidBrush brush = new SolidBrush(Color.FromArgb(160, 110, 60)))
                            {
                                g.FillRectangle(brush, rect);
                            }
                            using (Pen pen = new Pen(Color.FromArgb(200, 150, 80), 2))
                            {
                                g.DrawRectangle(pen, rect);
                            }
                            break;

                        case TileType.DoorClosed:
                            using (SolidBrush brush = new SolidBrush(Color.FromArgb(100, 70, 40)))
                            {
                                g.FillRectangle(brush, rect);
                            }
                            break;

                        case TileType.Empty:
                            using (SolidBrush brush = new SolidBrush(Color.FromArgb(40, 40, 40)))
                            {
                                g.FillRectangle(brush, rect);
                            }
                            break;
                    }
                }
            }
        }
    }

    public enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
}