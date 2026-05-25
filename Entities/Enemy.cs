using game.core;
using game.Environment;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace game.Entities
{
    public abstract class Enemy : GameObject
    {
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public float Speed { get; set; }
        protected Player target;
        protected List<Enemy> allEnemies;
        protected TileMap tileMap;
        private float decisionTimer = 0;
        private int strafeDirection = 1;
        private Random rnd = new Random();

        public Enemy(float x, float y, Player target, int health, float speed, int width, int height)
        {
            X = x; Y = y;
            this.target = target;
            Health = MaxHealth = health;
            Speed = speed;
            Width = width; Height = height;
            IsAlive = true;
            strafeDirection = rnd.Next(0, 2) == 0 ? -1 : 1;
        }

        public void SetReferences(List<Enemy> enemies, TileMap map) { allEnemies = enemies; tileMap = map; }

        public override void Update()
        {
            if (!target.IsAlive || !IsAlive) return;

            float dx = target.X - X;
            float dy = target.Y - Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            float angleToPlayer = (float)Math.Atan2(dy, dx);

            decisionTimer -= 1f / 60f;
            if (decisionTimer <= 0)
            {
                strafeDirection = rnd.Next(0, 2) == 0 ? -1 : 1;
                decisionTimer = (float)(rnd.NextDouble() * 2 + 1);
            }

            float finalAngle = angleToPlayer;

            if (dist > 350)
            {
                finalAngle = angleToPlayer;
            }
            else
            {
                float strafeIntensity = Math.Max(0, (dist - 50) / 300f);
                float strafeAngle = angleToPlayer + (float)(Math.PI / 2 * strafeDirection * strafeIntensity);
                finalAngle = LerpAngle(angleToPlayer, strafeAngle, 0.5f);
            }

            float moveAngle = AvoidObstacles(finalAngle);

            float vx = (float)Math.Cos(moveAngle) * Speed;
            float vy = (float)Math.Sin(moveAngle) * Speed;

            MoveSafe(vx, vy);
        }

        private float LerpAngle(float a, float b, float t)
        {
            float diff = b - a;
            while (diff < -Math.PI) diff += (float)Math.PI * 2;
            while (diff > Math.PI) diff -= (float)Math.PI * 2;
            return a + diff * t;
        }

        protected float AvoidObstacles(float desiredAngle)
        {
            if (tileMap == null) return desiredAngle;
            float rayDist = 50f;
            float sideAngle = 0.6f;

            bool center = IsWallInDirection(desiredAngle, rayDist);
            bool left = IsWallInDirection(desiredAngle - sideAngle, rayDist * 0.7f);
            bool right = IsWallInDirection(desiredAngle + sideAngle, rayDist * 0.7f);

            if (!center && !left && !right) return desiredAngle;

            if (left && !right) return desiredAngle + 0.5f;
            if (right && !left) return desiredAngle - 0.5f;
            if (center) return left ? desiredAngle + 0.8f : desiredAngle - 0.8f;

            return desiredAngle;
        }

        private bool IsWallInDirection(float angle, float distance)
        {
            float lookX = X + Width / 2 + (float)Math.Cos(angle) * distance;
            float lookY = Y + Height / 2 + (float)Math.Sin(angle) * distance;
            return tileMap.CheckCollisionWithTileMap(new RectangleF(lookX - 10, lookY - 10, 20, 20));
        }

        protected void MoveSafe(float dx, float dy)
        {
            float oldX = X;
            if (tileMap == null || tileMap.CanMoveTo(GetBounds(), dx, 0)) X += dx;
            if (CheckEnemyCollision()) X = oldX;

            float oldY = Y;
            if (tileMap == null || tileMap.CanMoveTo(GetBounds(), 0, dy)) Y += dy;
            if (CheckEnemyCollision()) Y = oldY;
        }

        protected bool CheckEnemyCollision()
        {
            if (allEnemies == null) return false;
            foreach (var other in allEnemies)
                if (other != this && other.IsAlive && GetBounds().IntersectsWith(other.GetBounds())) return true;
            return false;
        }

        public void ApplyKnockback(float kX, float kY)
        {
            float steps = Math.Max(Math.Abs(kX), Math.Abs(kY));
            if (steps < 1) steps = 1;
            float sX = kX / steps; float sY = kY / steps;
            for (int i = 0; i < (int)steps; i++)
            {
                if (tileMap != null && tileMap.CheckCollisionWithTileMap(new RectangleF(X + sX, Y + sY, Width, Height))) break;
                X += sX; Y += sY;
                if (CheckEnemyCollision()) break;
            }
        }

        protected void DrawHealthBar(Graphics g, int yOffset = 10, int barHeight = 4)
        {
            if (Health <= 0) return;
            float pct = (float)Health / MaxHealth;
            g.FillRectangle(Brushes.Red, X, Y - yOffset, Width * pct, barHeight);
            g.DrawRectangle(Pens.Black, X, Y - yOffset, Width, barHeight);
        }

        public void TakeDamage(int damage) { Health -= damage; if (Health <= 0) IsAlive = false; }
    }
}