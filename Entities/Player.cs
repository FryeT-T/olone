using game.core;
using game.Environment;
using game.Items;
using game.Items.Guns;
using game.Utils;
using game.Weapons;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace game.Entities
{
    public class Player : GameObject
    {
        public int Health { get; private set; } = 5;
        public int MaxHealth { get; private set; } = 5;
        public float Speed = 10f;
        public Inventory Inventory = new Inventory();
        private List<NPC> currentNPCs;
        private List<Enemy> currentEnemies;

        private float _shootCooldown = 0;
        private float _secondaryShootCooldown = 0;
        private AnimationController animationController;
        private float currentMoveX, currentMoveY;  
        private TileMap tileMap;

        public Player(float startX, float startY)
        {
            X = startX; Y = startY;
            Width = 60; Height = 70;
            CollisionKnockback = 35;
            ProjectileKnockback = 15;
            animationController = new AnimationController(frameDelaySeconds: 0.08f);
            animationController.LoadAllDirections("img/player/walk", frameCount: 6, prefix: "frame_00");
            animationController.LoadIdleSprites("img/player/rotations");
        }

        public void SetTileMap(TileMap map) => tileMap = map;
        public void SetNPCs(List<NPC> npcs) => currentNPCs = npcs;
        public void SetEnemies(List<Enemy> enemies) => currentEnemies = enemies;
        public void SetHealth(int hp, int max) { Health = hp; MaxHealth = max; }

        public override void Update()
        {
            if (!IsAlive) return;

            HandleWeaponSwitching();
            HandleMovement();
            HandleCombat();

            if (_shootCooldown > 0) _shootCooldown -= 1f / 60f;
            if (_secondaryShootCooldown > 0) _secondaryShootCooldown -= 1f / 60f;
        }

        private void HandleWeaponSwitching()
        {
            if (GameWindow.isInventoryOpen) return;

            int nextIndex = -1;
            if (InputManager.IsKeyDown(Keys.D1)) nextIndex = 0;
            if (InputManager.IsKeyDown(Keys.D2)) nextIndex = 1;
            if (nextIndex != -1 && nextIndex != Inventory.CurrentWeaponIndex)
            {
                Inventory.GetCurrentWeapon()?.CancelReload();

                Inventory.CurrentWeaponIndex = nextIndex;
            }
        }

        private void HandleMovement()
        {
            float moveX = 0, moveY = 0;
            if (InputManager.IsKeyDown(Keys.W)) moveY -= Speed;
            if (InputManager.IsKeyDown(Keys.S)) moveY += Speed;
            if (InputManager.IsKeyDown(Keys.A)) moveX -= Speed;
            if (InputManager.IsKeyDown(Keys.D)) moveX += Speed;

            if (moveX != 0 && moveY != 0) { moveX *= 0.707f; moveY *= 0.707f; }

            currentMoveX = moveX;
            currentMoveY = moveY;
            MoveSafe(moveX, 0);
            MoveSafe(0, moveY);
        }

        private void HandleCombat()
        {
            if (GameWindow.isInventoryOpen) return;

            Weapon weapon = Inventory.GetCurrentWeapon();
            if (weapon == null) return;

            weapon.UpdateReload(1f / 60f);
            if (InputManager.IsKeyDown(Keys.R)) weapon.StartReload();

            PointF dir = GetMouseDir();

            if (InputManager.IsMouseLeftPressed && _shootCooldown <= 0 && !weapon.IsReloading)
            {
                if (weapon.CurrentAmmo > 0)
                {
                    weapon.Shoot(X + Width / 2, Y + Height / 2, dir.X, dir.Y);
                    _shootCooldown = weapon.ShootDelay;
                }
                else weapon.StartReload();
            }

            if (InputManager.IsMouseRightPressed && _secondaryShootCooldown <= 0 && !weapon.IsReloading)
            {
                if (weapon.CurrentAmmo > 0)
                {
                    weapon.SecondaryShoot(X + Width / 2, Y + Height / 2, dir.X, dir.Y);
                    _secondaryShootCooldown = weapon.SecondaryShootDelay;
                }
                else weapon.StartReload();
            }
        }

        private void MoveSafe(float dx, float dy)
        {
            if (dx == 0 && dy == 0) return;
            RectangleF nextBounds = new RectangleF(X + dx, Y + dy, Width, Height);

            if (tileMap != null && tileMap.CheckCollisionWithTileMap(nextBounds)) return;

            if (currentNPCs != null)
                foreach (var npc in currentNPCs)
                    if (npc.IsAlive && nextBounds.IntersectsWith(npc.GetBounds())) return;

            if (currentEnemies != null)
                foreach (var en in currentEnemies)
                    if (en.IsAlive && nextBounds.IntersectsWith(en.GetBounds())) return;

            X += dx; Y += dy;
        }

        private PointF GetMouseDir()
        {
            float dx = InputManager.MouseX - (X + Width / 2);
            float dy = InputManager.MouseY - (Y + Height / 2);
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            return length > 0 ? new PointF(dx / length, dy / length) : new PointF(1, 0);
        }

        public override void Draw(Graphics g)
        {
            animationController.Update(1f / 60f, currentMoveX, currentMoveY);

            Bitmap currentSprite = animationController.GetCurrentFrame();

            if (currentSprite != null)
                g.DrawImage(currentSprite, X, Y, Width, Height);
            else
                g.FillRectangle(Brushes.Blue, X, Y, Width, Height);

            float hpP = (float)Health / MaxHealth;
            g.FillRectangle(Brushes.Red, X, Y - 10, Width * hpP, 5);
            g.DrawRectangle(Pens.Black, X, Y - 10, Width, 5);
        }
        public void Dispose()
        {
            animationController?.Dispose();
        }

        public void TakeDamage(int dmg) { Health = Math.Max(0, Health - dmg); if (Health <= 0) IsAlive = false; }
        public void ApplyKnockback(float kX, float kY)
        {
            if (tileMap == null || !tileMap.CheckCollisionWithTileMap(new RectangleF(X + kX, Y + kY, Width, Height)))
            {
                X += kX; Y += kY;
            }
        }
    }
}