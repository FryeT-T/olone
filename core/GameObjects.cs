using System.Drawing;

namespace game.core
{
    public abstract class GameObject
    {
        public float X { get; set; }
        public float Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsAlive { get; set; } = true;
        public float CollisionKnockback { get; set; } = 0;
        public float ProjectileKnockback { get; set; } = 0;
        public float CollisionDamageDeal { get; set; } = 0;
        public float CollisionDamageTakeMultiplier { get; set; } = 1;
        public float CollisionDamageTakeReduction { get; set; } = 0;
        public float AllDamageDealMultiplier { get; set; } = 1;
        public float AllDamageTakeMultiplier { get; set; } = 1;

        public RectangleF GetBounds() => new RectangleF(X, Y, Width, Height);

        public abstract void Update();
        public abstract void Draw(Graphics g);

        public Point GetCenter()
        {
            return new Point((int)X + Width / 2, (int)Y + Height / 2);
        }
    }
}