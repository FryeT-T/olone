using System;
using System.Drawing;
using game.core;

namespace game.Entities.Enemies
{
    public class Slime : Enemy
    {
        private enum SlimeState { Crawling, Windup, Hopping }
        private SlimeState currentState = SlimeState.Crawling;

        private float stateTimer = 0;
        private float hopDirX, hopDirY;
        private float hopProgress = 0;


        public Slime(float x, float y, Player target)
            : base(x, y, target, 1, 1.2f, 25, 25) 
        {
            CollisionDamageDeal = 1;
            CollisionDamageTakeReduction = 0;
        }

        public override void Update()
        {
            if (!IsAlive || !target.IsAlive) return;

            stateTimer += 1f / 60f;

            switch (currentState)
            {
                case SlimeState.Crawling:
                    
                    Speed = 2.8f;
                    base.Update();
                    if (stateTimer >= 1.5f) { currentState = SlimeState.Windup; stateTimer = 0; }
                    break;

                case SlimeState.Windup:
                    if (stateTimer >= 0.5f)
                    {
                        currentState = SlimeState.Hopping;
                        stateTimer = 0;
                        float dx = target.X - X;
                        float dy = target.Y - Y;
                        float dist = (float)Math.Sqrt(dx * dx + dy * dy);
                        hopDirX = (dx / dist);
                        hopDirY = (dy / dist);
                    }
                    break;

                case SlimeState.Hopping:
                    float hopSpeed = 7.0f; 

                    float nextX = hopDirX * hopSpeed;
                    float nextY = hopDirY * hopSpeed;

                    RectangleF futureBounds = new RectangleF(X + nextX, Y + nextY, Width, Height);
                    bool wallHit = tileMap != null && tileMap.CheckCollisionWithTileMap(futureBounds);

                    bool enemyHit = false;
                    if (allEnemies != null)
                    {
                        foreach (var other in allEnemies)
                            if (other != this && other.IsAlive && futureBounds.IntersectsWith(other.GetBounds()))
                            {
                                enemyHit = true; break;
                            }
                    }

                    if (!wallHit && !enemyHit)
                    {
                        X += nextX;
                        Y += nextY;
                    }
                    else
                    {
                        currentState = SlimeState.Crawling;
                        stateTimer = 0;
                        hopProgress = 0;
                    }

                    hopProgress = stateTimer / 0.6f;
                    if (stateTimer >= 0.6f)
                    {
                        currentState = SlimeState.Crawling;
                        stateTimer = 0;
                        hopProgress = 0;
                    }
                    break;
            }
        }

        public override void Draw(Graphics g)
        {
            float jumpHeight = 0;
            float scale = 1.0f;

            if (currentState == SlimeState.Hopping)
            {
                jumpHeight = (float)(40 * hopProgress * (1 - hopProgress));
                scale = 1.0f + (float)Math.Sin(hopProgress * Math.PI) * 0.4f;
            }

            int drawW = (int)(Width * scale);
            int drawH = (int)(Height * scale);
            float offsetX = (drawW - Width) / 2;
            float offsetY = (drawH - Height) / 2;

            using (SolidBrush shadow = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
                g.FillEllipse(shadow, X, Y + Height - 5, Width, 10);

            using (SolidBrush b = new SolidBrush(currentState == SlimeState.Windup ? Color.Yellow : Color.LimeGreen))
            {
                g.FillEllipse(b, X - offsetX, Y - offsetY - jumpHeight, drawW, drawH);
            }

            DrawHealthBar(g, (int)jumpHeight + 10);
        }
    }
}