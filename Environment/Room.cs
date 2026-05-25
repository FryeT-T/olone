using game.core;
using game.Entities;
using game.Entities.Enemies;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace game.Environment
{
    public class Room
    {
        public Rectangle Bounds;
        public TileMap TileMap;
        public bool[] Doors = new bool[4];
        public bool IsActive = false;
        public bool IsCleared { get; private set; } = false;
        public List<Enemy> Enemies = new List<Enemy>();
        public RoomType RoomType { get; set; }
        public Point Position { get; set; }

        public List<PointF> PendingSpawnPositions = new List<PointF>();
        private bool nextWaveIsBoss = false;

        private const int doorWidth = 2;
        private int currentWave = 0;
        private List<WaveData> waves = new List<WaveData>();
        private bool hasStartedWaves = false;
        private Random random;
        public List<DroppedItem> ItemsOnFloor = new List<DroppedItem>();

        public Room(Rectangle bounds, int levelSeed, RoomType type = RoomType.Normal)
        {
            Bounds = bounds;
            RoomType = type;

            int roomSeed = levelSeed ^ (bounds.X * 73856093) ^ (bounds.Y * 19349663) ^ (int)type;
            random = new Random(roomSeed);

            TileMap = new TileMap(bounds.Width / 40, bounds.Height / 40);
            GenerateWaves();
        }

        private void GenerateWaves()
        {
            if (RoomType == RoomType.Empty || RoomType == RoomType.Shop || RoomType == RoomType.Treasure)
                return;

            int waveCount;
            if (RoomType == RoomType.Boss) waveCount = 1;
            else if (RoomType == RoomType.Start) waveCount = random.Next(1, 2);
            else waveCount = random.Next(1, 4);

            for (int w = 0; w < waveCount; w++)
            {
                int enemyCount = (RoomType == RoomType.Boss) ? 1 : random.Next(3, 7);
                waves.Add(new WaveData
                {
                    EnemiesToSpawn = enemyCount,
                    IsBossWave = (RoomType == RoomType.Boss && w == 0)
                });
            }
        }

        public Point GetSpawnPosition(Direction entryDirection)
        {
            int centerX = GameWindow.WindowWidth / 2;
            int centerY = GameWindow.WindowHeight / 2;

            switch (entryDirection)
            {
                case Direction.Up: return new Point(centerX, 150);
                case Direction.Down: return new Point(centerX, GameWindow.WindowHeight - 150);
                case Direction.Left: return new Point(150, centerY);
                case Direction.Right: return new Point(GameWindow.WindowWidth - 150, centerY);
                default: return new Point(centerX, centerY);
            }
        }

        public void Enter()
        {
            IsActive = true;
            if (IsCleared) { OpenDoors(); return; }

            if (!hasStartedWaves)
            {
                hasStartedWaves = true;
                ResetWaves();
                CloseDoors();
            }
        }

        public void Exit() => IsActive = false;

        public void ResetWaves()
        {
            currentWave = 0;
            Enemies.Clear();
            PendingSpawnPositions.Clear();
        }

        public void ForceClear()
        {
            currentWave = waves.Count;
            IsCleared = true;
            hasStartedWaves = true;
            PendingSpawnPositions.Clear();
            Enemies.Clear();
            OpenDoors();
        }

        public void PrepareNextWave(Player player)
        {
            PendingSpawnPositions.Clear();
            if (currentWave >= waves.Count) return;

            var wave = waves[currentWave];
            nextWaveIsBoss = wave.IsBossWave;

            for (int i = 0; i < wave.EnemiesToSpawn; i++)
            {
                float x, y;
                int attempts = 0;
                do
                {
                    x = random.Next(150, Bounds.Width - 150);
                    y = random.Next(150, Bounds.Height - 150);
                    attempts++;
                } while (attempts < 50 && (Math.Abs(x - player.X) < 200 && Math.Abs(y - player.Y) < 200));

                PendingSpawnPositions.Add(new PointF(x, y));
            }
        }

        public void SpawnPreparedEnemies(Player player)
        {
            Enemies.Clear();
            foreach (var pos in PendingSpawnPositions)
            {
                Enemy enemy;
                if (nextWaveIsBoss)
                {
                    enemy = new Boss1(pos.X, pos.Y, player);
                }
                else
                {
                    int roll = random.Next(100);
                    if (roll < 15) enemy = new Orc(pos.X, pos.Y, player);
                    else if (roll < 35) enemy = new Skeleton(pos.X, pos.Y, player);
                    else if (roll < 55) enemy = new Slime(pos.X, pos.Y, player);
                    else enemy = new Crawler(pos.X, pos.Y, player);
                }
                Enemies.Add(enemy);
            }
            PendingSpawnPositions.Clear();
            currentWave++;
        }

        public bool IsWaveCleared() => Enemies.Count(e => e.IsAlive) == 0;
        public bool IsAllWavesCleared() => currentWave >= waves.Count && IsWaveCleared();

        public void CheckCleared()
        {
            if (!IsCleared && IsWaveCleared() && IsAllWavesCleared())
            {
                IsCleared = true;
                OpenDoors();
            }
        }

        public int CurrentWaveNumber => currentWave;
        public int TotalWaves => waves.Count;
        public int GetTotalEnemiesInCurrentWave() => (currentWave > 0 && currentWave <= waves.Count) ? waves[currentWave - 1].EnemiesToSpawn : 0;
        public int GetRemainingEnemiesInCurrentWave() => Enemies.Count(e => e.IsAlive);

        private bool IsNearDoor(int x, int y)
        {
            if (Doors[0] && y <= 2 && Math.Abs(x - TileMap.Width / 2) <= 3) return true;
            if (Doors[2] && y >= TileMap.Height - 3 && Math.Abs(x - TileMap.Width / 2) <= 3) return true;
            if (Doors[3] && x <= 2 && Math.Abs(y - TileMap.Height / 2) <= 3) return true;
            if (Doors[1] && x >= TileMap.Width - 3 && Math.Abs(y - TileMap.Height / 2) <= 3) return true;
            return false;
        }

        public void GenerateLayout()
        {
            for (int x = 0; x < TileMap.Width; x++)
                for (int y = 0; y < TileMap.Height; y++)
                    TileMap.tiles[x, y] = (x == 0 || x == TileMap.Width - 1 || y == 0 || y == TileMap.Height - 1) ? TileType.Wall : TileType.Empty;

            if (Doors[0]) SetDoor(TileMap.Width / 2, 0, Direction.Up, true);
            if (Doors[1]) SetDoor(TileMap.Width - 1, TileMap.Height / 2, Direction.Right, false);
            if (Doors[2]) SetDoor(TileMap.Width / 2, TileMap.Height - 1, Direction.Down, true);
            if (Doors[3]) SetDoor(0, TileMap.Height / 2, Direction.Left, false);

            AddObstacles();
        }

        private void SetDoor(int x, int y, Direction dir, bool horizontal)
        {
            int start = horizontal ? x - doorWidth / 2 : y - doorWidth / 2;
            for (int i = 0; i < doorWidth; i++)
            {
                if (horizontal) TileMap.tiles[start + i, y] = TileType.DoorOpened;
                else TileMap.tiles[x, start + i] = TileType.DoorOpened;
            }
            TileMap.SetDoorPosition(horizontal ? x : y, dir);
        }

        private void AddObstacles()
        {
            if (RoomType == RoomType.Shop || RoomType == RoomType.Treasure || RoomType == RoomType.Start) return;

            int clusters = random.Next(3, 7);
            for (int i = 0; i < clusters; i++)
            {
                int startX = random.Next(4, TileMap.Width - 4);
                int startY = random.Next(4, TileMap.Height - 4);
                int shapeType = random.Next(2);

                for (int dx = 0; dx < 2; dx++)
                {
                    for (int dy = 0; dy < 2; dy++)
                    {
                        if (shapeType == 0 && (dx > 0 || dy > 0)) continue;
                        int tx = startX + dx;
                        int ty = startY + dy;
                        if (!IsNearDoor(tx, ty)) TileMap.tiles[tx, ty] = TileType.Wall;
                    }
                }
            }
        }

        public void CloseDoors() => ToggleDoors(TileType.Wall);
        public void OpenDoors() => ToggleDoors(TileType.DoorOpened);

        private void ToggleDoors(TileType type)
        {
            if (Doors[0]) for (int i = 0; i < doorWidth; i++) TileMap.tiles[TileMap.Width / 2 - doorWidth / 2 + i, 0] = type;
            if (Doors[1]) for (int i = 0; i < doorWidth; i++) TileMap.tiles[TileMap.Width - 1, TileMap.Height / 2 - doorWidth / 2 + i] = type;
            if (Doors[2]) for (int i = 0; i < doorWidth; i++) TileMap.tiles[TileMap.Width / 2 - doorWidth / 2 + i, TileMap.Height - 1] = type;
            if (Doors[3]) for (int i = 0; i < doorWidth; i++) TileMap.tiles[0, TileMap.Height / 2 - doorWidth / 2 + i] = type;
        }
    }

    public class WaveData
    {
        public int EnemiesToSpawn { get; set; }
        public bool IsBossWave { get; set; }
    }
}