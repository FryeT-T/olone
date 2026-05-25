using System;
using System.Drawing;

namespace game.Entities
{
    public class Mannequin : NPC
    {
        public Mannequin(float x, float y) : base(x, y, 50, 70, 1000)
        {
        }

        public override void Draw(Graphics g)
        {
            using (SolidBrush b = new SolidBrush(Color.Sienna))
                g.FillRectangle(b, X, Y, Width, Height);

            g.FillRectangle(Brushes.DimGray, X - 10, Y + Height - 5, Width + 20, 10);

            float healthPercent = (float)Health / MaxHealth;
            healthPercent = Math.Max(0, Math.Min(1, healthPercent)); 

            int barWidth = 60; 
            float barX = X + (Width - barWidth) / 2;
            float barY = Y - 25;

            g.FillRectangle(Brushes.Black, barX, barY, barWidth, 8);
            g.FillRectangle(Brushes.Orange, barX, barY, barWidth * healthPercent, 8);
            g.DrawRectangle(Pens.White, barX, barY, barWidth, 8);

            using (Font f = new Font("Arial", 8, FontStyle.Bold))
            {
                string txt = $"{Health} HP";
                g.DrawString(txt, f, Brushes.White, barX, barY - 14);
            }
        }
    }
}