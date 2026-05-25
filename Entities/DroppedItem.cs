using System.Drawing;
using game.core;
using game.Items;

namespace game.Entities
{
    public class DroppedItem : GameObject
    {
        public Item Item { get; private set; }

        public DroppedItem(Item item, float x, float y)
        {
            Item = item;
            X = x;
            Y = y;
            Width = 30;
            Height = 30;
        }

        public override void Update() { }

        public override void Draw(Graphics g)
        {
            using (SolidBrush b = new SolidBrush(Color.Gold))
            {
                g.FillRectangle(b, X, Y, Width, Height);
            }
            g.DrawRectangle(Pens.White, X, Y, Width, Height);

            using (Font f = new Font("Arial", 8))
                g.DrawString(Item.Name, f, Brushes.White, X - 10, Y - 15);
        }
    }
}