using System.Drawing;
using game.core;

namespace game.Entities
{
    public abstract class NPC : GameObject
    {
        public int Health { get; set; }
        public int MaxHealth { get; set; }

        public NPC(float x, float y, int width, int height, int health)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Health = health;
            MaxHealth = health;
        }

        public virtual void TakeDamage(int damage)
        {
            Health -= damage;
            if (Health <= 0) IsAlive = false;
        }

        public override void Update() { }
    }
}