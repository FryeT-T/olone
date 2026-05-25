using System;
using System.Drawing;
using game.core;

namespace game.Weapons
{
    public class Projectile : GameObject
    {
        private float _dirX, _dirY;
        private float _speed = 20f;
        private int _damage;
        public float _projectileRange = 500;
        public float _currentRange = 0f;

        public Projectile(float x, float y, float dirX, float dirY, int size, int damage)
        {
            X = x - size / 2;
            Y = y - size / 2;
            Width = size;
            Height = size;
            _dirX = dirX;
            _dirY = dirY;
            _damage = damage;

        }
        public Projectile(float x, float y, float dirX, float dirY, int size, int damage, float projectileRange)
        {
            X = x - size / 2;
            Y = y - size / 2;
            Width = size;
            Height = size;
            _dirX = dirX;
            _dirY = dirY;
            _damage = damage;
            _projectileRange = projectileRange;
        }

        public override void Update()
        {
            X += _dirX * _speed;
            Y += _dirY * _speed;
            _currentRange += _speed;

            if (X < -100 || X > GameWindow.WindowWidth + 100 ||
                Y < -100 || Y > GameWindow.WindowHeight + 100|| _currentRange>=_projectileRange)
            {
                IsAlive = false;
            }
        }

        public override void Draw(Graphics g)
        {
            using (SolidBrush brush = new SolidBrush(Color.Yellow))
            {
                g.FillEllipse(brush, X, Y, Width, Height);
            }
        }

        public int GetDamage() => _damage;
    }
}