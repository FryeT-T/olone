using System;
using System.Drawing;
using game.core;
using game.Entities;

namespace game.Environment
{
    public class Teleport : GameObject
    {
        public Teleport(float x, float y)
        {
            X = x;
            Y = y;
            Width = 60;
            Height = 60;
        }

        public override void Update() { }

        public override void Draw(Graphics g)
        {
            using (Pen p = new Pen(Color.Cyan, 3))
            {
                g.DrawEllipse(p, X, Y, Width, Height);
                g.DrawEllipse(p, X + 10, Y + 10, Width - 20, Height - 20);
            }

            int alpha = 50 + (int)(30 * Math.Sin(DateTime.Now.Millisecond / 100.0));
            using (SolidBrush b = new SolidBrush(Color.FromArgb(alpha, Color.Cyan)))
            {
                g.FillEllipse(b, X - 5, Y - 5, Width + 10, Height + 10);
            }
        }

        public bool IsPlayerNear(Player p)
        {
            float centerX = X + Width / 2;
            float centerY = Y + Height / 2;
            float playerCenterX = p.X + p.Width / 2;
            float playerCenterY = p.Y + p.Height / 2;

            double dist = Math.Sqrt(Math.Pow(centerX - playerCenterX, 2) + Math.Pow(centerY - playerCenterY, 2));
            return dist < 80; 
        }
    }
}