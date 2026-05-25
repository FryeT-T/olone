using System;
using System.Drawing;
using game.core;
using game.Weapons;

namespace game.Entities.Enemies
{
    public class Skeleton : Enemy
    {
        private float shootTimer = 0;
        private float shootDelay = 2.0f;

        public Skeleton(float x, float y, Player target)
            : base(x, y, target, 3, 1.8f, 35, 35)
        {
            CollisionDamageDeal = 1;
        }

        public override void Update()
        {
            if (!target.IsAlive || !IsAlive) return;

            float dx = target.X - X;
            float dy = target.Y - Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            if (dist < 250)
            {
                float moveX = -(dx / dist) * Speed;
                float moveY = -(dy / dist) * Speed;

                MoveSafe(moveX, moveY);
            }
            else if (dist > 400)
            {
                base.Update();
            }

            shootTimer += 1f / 60f;
            if (shootTimer >= shootDelay && dist < 600)
            {
                float dirX = dx / dist;
                float dirY = dy / dist;
                GameWindow.AddEnemyProjectile(new Projectile(X + Width / 2, Y + Height / 2, dirX, dirY, 12, 1, 650));
                shootTimer = 0;
            }
        }

        public override void Draw(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(Color.MediumPurple))
            {
                g.FillRectangle(brush, X, Y, Width, Height);
            }
            DrawHealthBar(g, 10, 5);
        }
    }
}