using System;
using System.Drawing;

namespace game.Entities.Enemies
{
    public class Crawler : Enemy
    {
        private float zigZagTimer = 0;
        public Crawler(float x, float y, Player target)
            : base(x, y, target, 3, 2.0f, 35, 35)
        {
            CollisionKnockback = 10;
            ProjectileKnockback = 5;
            CollisionDamageDeal = 1;
        }

        public override void Update()
        {
            if (!IsAlive || !target.IsAlive) return;

            zigZagTimer += 0.1f;
            float offset = (float)Math.Sin(zigZagTimer) * 2.0f; 
            float dx = target.X - X;
            float dy = target.Y - Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            if (dist > 0)
            {
                float dirX = dx / dist;
                float dirY = dy / dist;

                float moveX = dirX * Speed + (-dirY * offset);
                float moveY = dirY * Speed + (dirX * offset);

                MoveSafe(moveX, moveY);
            }
        }

        public override void Draw(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(Color.DarkRed))
            {
                g.FillRectangle(brush, X, Y, Width, Height);
            }
            DrawHealthBar(g);
        }
    }
}