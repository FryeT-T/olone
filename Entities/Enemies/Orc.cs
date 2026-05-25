using game.core;
using System;
using System.Drawing;

namespace game.Entities.Enemies
{

    public class Orc : Enemy
    {
        private float chargeTimer = 0;
        private bool isCharging = false;
        private float chargeWindup = 0; 
        private float chargeDirX, chargeDirY;
        public Orc(float x, float y, Player target)
            : base(x, y, target, 12, 2.2f, 60, 60)
        {
            CollisionDamageDeal = 2;
            CollisionKnockback = 5;
            ProjectileKnockback = 3;
            AllDamageTakeMultiplier = 0.9f;
        }

        public override void Update()
        {
            if (!IsAlive || !target.IsAlive) return;

            float dx = target.X - X;
            float dy = target.Y - Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            if (!isCharging)
            {
                base.Update(); 
                chargeTimer += 1f / 60f;

                if (chargeTimer >= 3.0f && dist < 400)
                {
                    isCharging = true;
                    chargeWindup = 0.5f; 
                    chargeDirX = dx / dist;
                    chargeDirY = dy / dist;
                    chargeTimer = 0;
                }
            }
            else
            {
                if (chargeWindup > 0)
                {
                    chargeWindup -= 1f / 60f;
                }
                else
                {
                    MoveSafe(chargeDirX * 12, chargeDirY * 12);

                    chargeTimer += 1f / 60f;
                    if (chargeTimer >= 0.3f)
                    {
                        isCharging = false;
                        chargeTimer = 0;
                    }
                }
            }
        }

        public override void Draw(Graphics g)
        {
            Color c = isCharging && chargeWindup > 0 ? Color.OrangeRed : Color.DarkSlateGray;
            using (SolidBrush b = new SolidBrush(c)) g.FillRectangle(b, X, Y, Width, Height);
            DrawHealthBar(g, 12, 6);
        }
    }
}