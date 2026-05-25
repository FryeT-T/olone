using game.Entities;
using game.Environment;
using game.Items;
using game.Items.Guns;
using game.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace game.core
{
    public partial class GameWindow
    {
        private void GameLoop()
        {
            InputManager.Update(this);

            if (InputManager.IsKeyDown(Keys.Tab))
            {
                isInventoryOpen = !isInventoryOpen;
                if (!isInventoryOpen) ReturnDraggedItem();
                System.Threading.Thread.Sleep(150);
            }

            if (isInventoryOpen) UpdateInventoryLogic();
            HandleItemPickup();
            HandleItemDrop();

            switch (currentState)
            {
                case GameState.MainMenu:
                    if (InputManager.IsKeyDown(Keys.Enter)) InitializeLobby();
                    if (InputManager.IsKeyDown(Keys.S)) LoadGameProgress();
                    break;
                case GameState.Lobby:
                    UpdateLobbyLogic();
                    if (InputManager.IsKeyDown(Keys.Escape)) currentState = GameState.Paused;
                    break;
                case GameState.Playing:
                    UpdateGameLogic();
                    if (InputManager.IsKeyDown(Keys.Escape)) currentState = GameState.Paused;
                    break;
                case GameState.Paused:
                    if (InputManager.IsKeyDown(Keys.Enter)) currentState = (lobbyTeleport != null) ? GameState.Lobby : GameState.Playing;
                    if (InputManager.IsKeyDown(Keys.Back)) currentState = GameState.MainMenu;
                    if (InputManager.IsKeyDown(Keys.F5)) SaveGame(); 
                    break;
                case GameState.GameOver:
                    if (InputManager.IsKeyDown(Keys.R)) InitializeLobby();
                    break;
            }
            Invalidate();
        }

        private void UpdateLobbyLogic()
        {
            player.SetEnemies(enemies); 
            player.Update();
            foreach (var p in playerProjectiles) p.Update();

            foreach (var n in npcs)
            {
                foreach (var p in playerProjectiles)
                    if (n.IsAlive && p.GetBounds().IntersectsWith(n.GetBounds()))
                    { n.TakeDamage(p.GetDamage()); p.IsAlive = false; }
            }
            npcs.RemoveAll(n => !n.IsAlive);
            CleanupEntities();
            if (lobbyTeleport != null && lobbyTeleport.IsPlayerNear(player) && InputManager.IsKeyDown(Keys.F)) StartActualGame();
        }

        private void UpdateGameLogic()
        {
            player.SetEnemies(enemies);
            player.Update();
            CheckRoomTransition();

            foreach (var e in enemies) e.Update();
            foreach (var p in playerProjectiles) p.Update();

            foreach (var e in enemies)
            {
                if (e.IsAlive && player.GetBounds().IntersectsWith(e.GetBounds()))
                {
                    Collisions.PlayerCollidesWithEnemy(player, e);
                }
            }

            foreach (var p in playerProjectiles)
            {
                foreach (var e in enemies)
                {
                    if (p.IsAlive && e.IsAlive && p.GetBounds().IntersectsWith(e.GetBounds()))
                    {
                        e.TakeDamage(p.GetDamage());
                        p.IsAlive = false;
                        Collisions.ProjectileKnockback(p, e);
                        break;
                    }
                }
            }
            foreach (var p in enemyProjectiles)
            {
                p.Update();
                if (p.IsAlive && p.GetBounds().IntersectsWith(player.GetBounds()))
                {
                    player.TakeDamage(p.GetDamage());
                    p.IsAlive = false;
                    Collisions.ProjectileKnockback(p, player);
                }
            }

            CleanupEntities();
            UpdateWaves();
            if (!player.IsAlive) currentState = GameState.GameOver;
        }

        private void UpdateWaves()
        {
            if (currentRoom != null && !currentRoom.IsCleared)
            {
                currentRoom.CheckCleared();

                if (currentRoom.IsCleared)
                {
                    SaveGame();
                }

                if (currentRoom.IsWaveCleared() && !currentRoom.IsAllWavesCleared())
                {
                    waveDelayTimer++;
                    if (waveDelayTimer == 30) currentRoom.PrepareNextWave(player);
                    if (waveDelayTimer >= 60)
                    {
                        currentRoom.SpawnPreparedEnemies(player);
                        foreach (var e in currentRoom.Enemies) { enemies.Add(e); e.SetReferences(enemies, tileMap); }
                        waveDelayTimer = 0;
                    }
                }
            }
        }

        private void LoadRoom(Point roomPos, Direction entryDirection = Direction.None)
        {
            if (!rooms.ContainsKey(roomPos)) return;

            if (currentRoom != null) currentRoom.Exit();
            currentRoom = rooms[roomPos];
            currentRoomPos = roomPos;
            currentRoom.Enter();
            tileMap = currentRoom.TileMap;
            player.SetTileMap(tileMap);

            Point spawnPos = currentRoom.GetSpawnPosition(entryDirection);
            player.X = spawnPos.X;
            player.Y = spawnPos.Y;

            enemies.Clear();
            if (!currentRoom.IsCleared) { currentRoom.PrepareNextWave(player); waveDelayTimer = 30; }
            else currentRoom.OpenDoors();

            if (currentState == GameState.Playing) SaveGame();
        }

        private void InitializeLobby()
        {
            currentLevel = new LevelData(1, 1,0);
            currentLevel.StartRoom = new Point(0, 0);
            currentLevel.RoomLayout[0, 0] = RoomType.Start;
            GenerateLevelRooms();
            player = new Player(WindowWidth / 2, WindowHeight / 2);
            LoadRoom(currentLevel.StartRoom, Direction.None);
            if (currentRoom != null) currentRoom.ForceClear();
            npcs.Clear();
            npcs.Add(new Mannequin(WindowWidth / 2 + 400, WindowHeight / 2));
            player.SetNPCs(npcs);
            lobbyTeleport = new Teleport(WindowWidth / 2 - 30, WindowHeight / 2 - 30);
            currentRoom.ItemsOnFloor.Add(new DroppedItem(new Pistol(), player.X - 100, player.Y + 100));
            currentRoom.ItemsOnFloor.Add(new DroppedItem(new Shotgun(), player.X, player.Y + 100));
            currentRoom.ItemsOnFloor.Add(new DroppedItem(new SniperRifle(), player.X + 100, player.Y + 100));
            currentState = GameState.Lobby;
        }

        private void StartActualGame()
        {
            npcs.Clear(); enemies.Clear(); playerProjectiles.Clear();
            currentLevel = new LevelGenerator().GenerateLevel(5, 5);
            GenerateLevelRooms();
            LoadRoom(currentLevel.StartRoom, Direction.None);
            lobbyTeleport = null;
            currentState = GameState.Playing;
        }

        private void GenerateLevelRooms()
        {
            rooms.Clear();
            for (int x = 0; x < currentLevel.Width; x++)
                for (int y = 0; y < currentLevel.Height; y++)
                    if (currentLevel.HasRoom(x, y))
                    {
                        var room = new Room(
                            new Rectangle(x * WindowWidth, y * WindowHeight, WindowWidth, WindowHeight),
                            currentLevel.Seed,
                            currentLevel.GetRoomType(x, y)
                        );
                        room.Position = new Point(x, y);
                        room.Doors[0] = currentLevel.HasRoom(x, y - 1);
                        room.Doors[1] = currentLevel.HasRoom(x + 1, y);
                        room.Doors[2] = currentLevel.HasRoom(x, y + 1);
                        room.Doors[3] = currentLevel.HasRoom(x - 1, y);
                        room.GenerateLayout();
                        rooms[new Point(x, y)] = room;
                    }
        }
        private void HandleItemPickup()
        {
            if (InputManager.IsKeyDown(Keys.F))
            {
                float pickupRadius = 80f;
                float mouseClickRadius = 20f;

                DroppedItem itemToPick = null;

                foreach (var dropped in currentRoom.ItemsOnFloor)
                {
                    float distToPlayer = GetDistance(player.X + player.Width / 2, player.Y + player.Height / 2,
                                                    dropped.X + dropped.Width / 2, dropped.Y + dropped.Height / 2);

                    float distToMouse = GetDistance(InputManager.MouseX, InputManager.MouseY,
                                                   dropped.X + dropped.Width / 2, dropped.Y + dropped.Height / 2);

                    if (distToPlayer <= pickupRadius && distToMouse <= mouseClickRadius)
                    {
                        itemToPick = dropped;
                        break;
                    }
                }

                if (itemToPick != null)
                {
                    if (player.Inventory.TryAddItem(itemToPick.Item))
                    {
                        currentRoom.ItemsOnFloor.Remove(itemToPick);
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
        }
        private void HandleItemDrop()
        {
            if (InputManager.IsKeyDown(Keys.G)&& player.Inventory.GetCurrentWeapon() !=null)
            {
                currentRoom.ItemsOnFloor.Add(new DroppedItem(player.Inventory.TakeItemFromWeaponSlots(player.Inventory.CurrentWeaponIndex), player.X, player.Y));
                System.Threading.Thread.Sleep(100);
            }
        }

        private float GetDistance(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        private void LoadGameProgress()
        {
            var data = SaveManager.Load();
            if (data == null) return;

            npcs.Clear(); enemies.Clear(); playerProjectiles.Clear();

            currentLevel = new LevelData(data.LevelWidth, data.LevelHeight, data.Seed);
            for (int x = 0; x < data.LevelWidth; x++)
                for (int y = 0; y < data.LevelHeight; y++)
                    currentLevel.RoomLayout[x, y] = (RoomType)data.LayoutRaw[x][y];

            GenerateLevelRooms();
            foreach (var coord in data.ClearedRooms)
            {
                string[] parts = coord.Split(',');
                Point p = new Point(int.Parse(parts[0]), int.Parse(parts[1]));
                if (rooms.ContainsKey(p)) rooms[p].ForceClear();
            }

            player = new Player(0, 0);
            player.SetHealth(data.PlayerHealth, data.PlayerMaxHealth);

            for (int i = 0; i < 2; i++) player.Inventory.WeaponSlots[i] = CreateWeaponByName(data.WeaponSlotNames[i], data.WeaponSlotAmmo[i]);
            for (int i = 0; i < 15; i++) player.Inventory.MainSlots[i] = CreateWeaponByName(data.MainSlotNames[i], data.MainSlotAmmo[i]);
            player.Inventory.CurrentWeaponIndex = data.CurrentWeaponIndex;

            currentRoomPos = new Point(data.CurrentRoomX, data.CurrentRoomY);

            LoadRoom(currentRoomPos, Direction.None);

            player.X = data.PlayerX;
            player.Y = data.PlayerY;

            lobbyTeleport = null;
            currentState = GameState.Playing;
        }

        private Weapon CreateWeaponByName(string name, int ammo)
        {
            if (string.IsNullOrEmpty(name)) return null;

            Weapon w = null;
            if (name == "Pistol") w = new game.Items.Guns.Pistol();
            else if (name == "Shotgun") w = new game.Items.Guns.Shotgun();
            else if (name == "SniperRifle") w = new game.Items.Guns.SniperRifle();

            if (w != null) w.CurrentAmmo = ammo;
            return w;
        }

        private void SaveGame()
        {
            if (currentState == GameState.MainMenu || player == null) return;

            try
            {
                var data = new SaveData
                {
                    Seed = currentLevel.Seed,
                    PlayerX = player.X,
                    PlayerY = player.Y,
                    PlayerHealth = player.Health,
                    PlayerMaxHealth = player.MaxHealth,
                    CurrentRoomX = currentRoomPos.X,
                    CurrentRoomY = currentRoomPos.Y,
                    LevelWidth = currentLevel.Width,
                    LevelHeight = currentLevel.Height,
                    CurrentWeaponIndex = player.Inventory.CurrentWeaponIndex,

                    WeaponSlotNames = new string[2],
                    WeaponSlotAmmo = new int[2],
                    MainSlotNames = new string[15],
                    MainSlotAmmo = new int[15],
                    ClearedRooms = new List<string>()
                };

                if (currentLevel != null && currentLevel.RoomLayout != null)
                {
                    data.LayoutRaw = new int[currentLevel.Width][];
                    for (int x = 0; x < currentLevel.Width; x++)
                    {
                        data.LayoutRaw[x] = new int[currentLevel.Height];
                        for (int y = 0; y < currentLevel.Height; y++)
                        {
                            data.LayoutRaw[x][y] = (int)currentLevel.RoomLayout[x, y];
                            Point p = new Point(x, y);
                            if (rooms.ContainsKey(p) && rooms[p].IsCleared)
                                data.ClearedRooms.Add($"{x},{y}");
                        }
                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    var w = player.Inventory.WeaponSlots[i];
                    if (w != null) { data.WeaponSlotNames[i] = w.GetType().Name; data.WeaponSlotAmmo[i] = w.CurrentAmmo; }
                }
                for (int i = 0; i < 15; i++)
                {
                    var it = player.Inventory.MainSlots[i];
                    if (it != null) { data.MainSlotNames[i] = it.GetType().Name; if (it is Weapon w) data.MainSlotAmmo[i] = w.CurrentAmmo; }
                }

                SaveManager.Save(data);

                Console.WriteLine("Игра успешно сохранена вручную!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        private void CleanupEntities()
        {
            enemies.RemoveAll(e => !e.IsAlive);
            playerProjectiles.RemoveAll(p => !p.IsAlive);
            playerProjectiles.AddRange(pendingProjectiles);
            pendingProjectiles.Clear();
            enemyProjectiles.RemoveAll(p => !p.IsAlive);
            enemyProjectiles.AddRange(pendingEnemyProjectiles);
            pendingEnemyProjectiles.Clear();
        }

        private void CheckRoomTransition()
        {
            if (tileMap.IsOnDoorTransition(player.GetBounds(), out Direction dir))
            {
                Point nPos = currentRoomPos;
                if (dir == Direction.Up) nPos.Y--;
                else if (dir == Direction.Down) nPos.Y++;
                else if (dir == Direction.Left) nPos.X--;
                else if (dir == Direction.Right) nPos.X++;

                if (rooms.ContainsKey(nPos))
                    LoadRoom(nPos, dir == Direction.Up ? Direction.Down : (dir == Direction.Down ? Direction.Up : (dir == Direction.Left ? Direction.Right : Direction.Left)));
            }
        }
    }
}