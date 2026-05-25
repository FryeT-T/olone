using game.core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Environment
{
    public class Wall : GameObject
    {
        public Wall(float x, float y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public override void Update()
        {
        }

        public override void Draw(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(Color.Gray))
            {
                g.FillRectangle(brush, X, Y, Width, Height);
            }
            using (Pen pen = new Pen(Color.Black, 2))
            {
                g.DrawRectangle(pen, X, Y, Width, Height);
            }
        }
    }
}
