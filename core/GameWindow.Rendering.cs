using game.Environment;
using game.Items;
using game.Weapons;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace game.core
{
    public partial class GameWindow
    {
        private void OnPaint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (currentState == GameState.MainMenu)
            {
                DrawMainMenu(g);
                return;
            }

            tileMap?.Draw(g);

            if (currentState == GameState.Lobby && lobbyTeleport != null)
            {
                lobbyTeleport.Draw(g);
                if (lobbyTeleport.IsPlayerNear(player))
                {
                    using (Font f = new Font("Arial", 14, FontStyle.Bold))
                    {
                        g.DrawString("Нажмите [F], чтобы войти в подземелье", f, Brushes.Black, player.X - 49, player.Y - 41);
                        g.DrawString("Нажмите [F], чтобы войти в подземелье", f, Brushes.Cyan, player.X - 50, player.Y - 40);
                    }
                }
            }

            if (currentState == GameState.Playing) DrawSpawnIndicators(g);
            foreach (var dropped in currentRoom.ItemsOnFloor)
            {
                dropped.Draw(g);
                float dP = GetDistance(player.X, player.Y, dropped.X, dropped.Y);
                float dM = GetDistance(InputManager.MouseX, InputManager.MouseY, dropped.X + 15, dropped.Y + 15);

                if (dP < 80 && dM < 20f)
                {
                    g.DrawRectangle(new Pen(Color.Cyan, 2), dropped.X - 2, dropped.Y - 2, dropped.Width + 4, dropped.Height + 4);
                    using (Font f = new Font("Arial", 10, FontStyle.Bold))
                        g.DrawString("[F] Подобрать", f, Brushes.Cyan, dropped.X - 15, dropped.Y - 30);
                }
            }
            player?.Draw(g);
            foreach (var enemy in enemies) enemy.Draw(g);
            foreach (var n in npcs) n.Draw(g);
            foreach (var projectile in playerProjectiles) projectile.Draw(g);
            foreach (var p in enemyProjectiles) p.Draw(g);

            DrawUI(g);
            if (currentState != GameState.MainMenu) DrawWeaponHUD(g);
            if (isInventoryOpen) DrawInventory(g);

            if (currentState == GameState.Paused) DrawPauseOverlay(g);

            if (currentState == GameState.GameOver) DrawGameOverOverlay(g);
        }

        private void DrawMinimap(Graphics g)
        {
            if (currentState != GameState.Playing || currentLevel == null) return;
            int mx = WindowWidth - 220, my = 20, cellSize = 18;
            for (int x = 0; x < currentLevel.Width; x++)
                for (int y = 0; y < currentLevel.Height; y++)
                    if (currentLevel.HasRoom(x, y))
                    {
                        Color c = Color.FromArgb(100, 100, 100);
                        RoomType type = currentLevel.GetRoomType(x, y);
                        if (type == RoomType.Boss) c = Color.Red;
                        else if (type == RoomType.Shop) c = Color.Cyan;
                        else if (type == RoomType.Treasure) c = Color.Gold;
                        else if (type == RoomType.Start) c = Color.White;

                        if (rooms.ContainsKey(new Point(x, y)) && rooms[new Point(x, y)].IsCleared)
                            c = Color.FromArgb(c.R / 2, 255, c.B / 2);

                        if (x == currentRoomPos.X && y == currentRoomPos.Y) c = Color.Yellow;
                        g.FillRectangle(new SolidBrush(c), mx + x * cellSize, my + y * cellSize, cellSize - 2, cellSize - 2);
                        g.DrawRectangle(Pens.Black, mx + x * cellSize, my + y * cellSize, cellSize - 2, cellSize - 2);
                    }
        }

        private void DrawWeaponHUD(Graphics g)
        {
            if (player == null) return;
            int sSize = 80, pad = 20;
            int startX = (WindowWidth / 2) - sSize - (pad / 2);
            int startY = WindowHeight - 120;
            for (int i = 0; i < 2; i++)
            {
                Rectangle r = new Rectangle(startX + i * (sSize + pad), startY, sSize, sSize);
                Weapon w = player.Inventory.WeaponSlots[i];
                bool active = player.Inventory.CurrentWeaponIndex == i;
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 20, 20, 20)), r);
                g.DrawRectangle(new Pen(active ? Color.Gold : Color.Gray, active ? 4 : 2), r);
                if (w != null)
                {
                    string ammo = w.IsReloading ? "..." : $"{w.CurrentAmmo}/{w.MaxAmmo}";
                    g.DrawString(w.Name, new Font("Arial", 8), Brushes.White, r.X + 5, r.Y + 5);
                    g.DrawString(ammo, new Font("Consolas", 12, FontStyle.Bold), w.CurrentAmmo == 0 ? Brushes.Red : Brushes.White, r.X + 15, r.Y + 30);
                    if (active && w.IsReloading) g.FillRectangle(Brushes.Cyan, r.X, r.Y - 10, r.Width * (1 - w.ReloadTimer / w.ReloadTime), 6);
                }
            }
        }

        private void DrawInventory(Graphics g)
        {
            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(180, 0, 0, 0))) g.FillRectangle(shadow, 0, 0, WindowWidth, WindowHeight);
            int startX = WindowWidth / 2 - 245, startY = WindowHeight / 2 - 150;
            for (int i = 0; i < 15; i++)
            {
                Rectangle r = new Rectangle(startX + (i % 5) * (invCellSize + invPad), startY + (i / 5) * (invCellSize + invPad), invCellSize, invCellSize);
                g.FillRectangle(new SolidBrush(Color.FromArgb(100, 100, 100)), r);
                g.DrawRectangle(new Pen(Color.Gray, 2), r);
                if (player.Inventory.MainSlots[i] != null) g.DrawString(player.Inventory.MainSlots[i].Name, new Font("Arial", 8, FontStyle.Bold), Brushes.White, r.X + 5, r.Y + 5);
            }
            int weaponStartY = startY + 3 * (invCellSize + invPad) + 30;
            for (int i = 0; i < 2; i++)
            {
                Rectangle r = new Rectangle(startX + i * (invCellSize + invPad), weaponStartY, invCellSize, invCellSize);
                g.FillRectangle(new SolidBrush(player.Inventory.CurrentWeaponIndex == i ? Color.DarkGoldenrod : Color.FromArgb(80, 80, 120)), r);
                g.DrawRectangle(new Pen(Color.Gold, 3), r);
                if (player.Inventory.WeaponSlots[i] != null) g.DrawString(player.Inventory.WeaponSlots[i].Name, new Font("Arial", 10, FontStyle.Bold), Brushes.White, r.X + 5, r.Y + 5);
            }
            if (draggedItem != null) g.DrawString(draggedItem.Name, new Font("Arial", 10, FontStyle.Bold), Brushes.Yellow, InputManager.MouseX + 15, InputManager.MouseY + 15);
            g.DrawString("ИНВЕНТАРЬ", new Font("Arial", 32, FontStyle.Bold), Brushes.White, startX, startY - 70);
        }

        private void DrawUI(Graphics g)
        {
            using (Font f = new Font("Arial", 16))
            {
                g.DrawString($" Здоровье: {player.Health}/{player.MaxHealth}", f, Brushes.LightGreen, 10, 10);
                if (currentState == GameState.Playing) g.DrawString($" Комната: {currentRoomPos.X},{currentRoomPos.Y}", new Font("Arial", 12), Brushes.White, 10, 40);
            }
            DrawMinimap(g);
        }

        private void DrawSpawnIndicators(Graphics g)
        {
            if (currentRoom == null) return;
            using (Pen p = new Pen(Color.Red, 3))
                foreach (var pos in currentRoom.PendingSpawnPositions)
                {
                    g.DrawLine(p, pos.X - 15, pos.Y - 15, pos.X + 15, pos.Y + 15);
                    g.DrawLine(p, pos.X + 15, pos.Y - 15, pos.X - 15, pos.Y + 15);
                }
        }

        private void DrawMainMenu(Graphics g)
        {
            g.Clear(Color.FromArgb(15, 15, 15));
            using (Font titleF = new Font("Impact", 72, FontStyle.Bold))
            using (Font btnF = new Font("Arial", 22, FontStyle.Regular))
            {
                int centerX = WindowWidth / 2 - 200;

                g.DrawString("OLONE", titleF, Brushes.Crimson, WindowWidth / 2 - 160, 180);

                g.DrawString("ENTER - Войти в лобби", btnF, Brushes.White, centerX, 400);

                Brush saveBrush = SaveManager.HasSave() ? Brushes.Gold : Brushes.Gray;
                g.DrawString("S - Загрузить сохранение", btnF, saveBrush, centerX, 460);

                g.DrawString("ESC - Выход из игры", btnF, Brushes.DarkGray, centerX, 520);
            }
        }

        private void DrawPauseOverlay(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(180, 0, 0, 0)), 0, 0, WindowWidth, WindowHeight);

            using (Font titleF = new Font("Arial", 42, FontStyle.Bold))
            using (Font btnF = new Font("Arial", 20))
            {
                int centerX = WindowWidth / 2 - 200;
                g.DrawString("ПАУЗА", titleF, Brushes.White, WindowWidth / 2 - 100, 250);

                g.DrawString("ENTER - Продолжить", btnF, Brushes.LightGreen, centerX, 400);
                g.DrawString("F5 - Сохранить прогресс", btnF, Brushes.Gold, centerX, 460);
                g.DrawString("BACKSPACE - В главное меню", btnF, Brushes.LightCoral, centerX, 520);

                g.DrawString("Игра сохраняется автоматически после каждой комнаты",
                             new Font("Arial", 10), Brushes.Gray, centerX, 600);
            }
        }

        private void DrawGameOverOverlay(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 100, 0, 0)), 0, 0, WindowWidth, WindowHeight);
            using (Font f = new Font("Arial", 48, FontStyle.Bold))
            using (Font fSmall = new Font("Arial", 24))
            {
                g.DrawString("ИГРА ОКОНЧЕНА", f, Brushes.White, WindowWidth / 2 - 250, WindowHeight / 2 - 100);
                g.DrawString("Нажмите [R], чтобы вернуться в лобби", fSmall, Brushes.White, WindowWidth / 2 - 250, WindowHeight / 2 + 20);
            }
        }
    }
}