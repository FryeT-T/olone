using game.core;
using System;
using System.Drawing;

namespace game.Entities.Enemies
{
    public class Boss1 : Enemy
    {
        private float specialAttackTimer = 0;
        public Boss1(float x, float y, Player target)
            : base(x, y, target, 20, 1.5f, 55, 55)
        {
            CollisionKnockback = 5;
            ProjectileKnockback = 2;
            CollisionDamageDeal = 2;
        }

        public override void Update()
        {
            base.Update();

            specialAttackTimer += 1f / 60f;
            if (specialAttackTimer >= 4.0f)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = (float)(i * Math.PI * 2 / 8);
                    float vx = (float)Math.Cos(angle);
                    float vy = (float)Math.Sin(angle);
                    GameWindow.AddEnemyProjectile(new Weapons.Projectile(X + Width / 2, Y + Height / 2, vx, vy, 15, 1));
                }
                specialAttackTimer = 0;
            }
        }

        public override void Draw(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(Color.Purple))
            {
                g.FillRectangle(brush, X, Y, Width, Height);
            }

            DrawHealthBar(g, 15, 8);

            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                string healthText = $"{Health}/{MaxHealth}";
                float textWidth = g.MeasureString(healthText, font).Width;
                g.DrawString(healthText, font, textBrush, X + (Width - textWidth) / 2, Y - 30);
            }
        }
    }
}