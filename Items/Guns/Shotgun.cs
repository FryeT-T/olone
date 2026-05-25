using game.core;
using game.Weapons;
using System;

namespace game.Items.Guns
{
    public class Shotgun : Weapon
    {
        public Shotgun()
        {
            Name = "Дробовик";
            ShootDelay = 0.8f;
            SecondaryShootDelay = 1.5f;
            MaxAmmo = 5;
            CurrentAmmo = 5;
            ReloadTime = 2.0f;
            Damage = 1;
            ProjectileSize = 6;
            ProjectileRange = 260;
        }

        public override void Shoot(float x, float y, float dirX, float dirY)
        {
            if (CurrentAmmo <= 0) return;
            CurrentAmmo--; 

            int pellets = 5;
            float spread = 0.3f;
            float baseAngle = (float)Math.Atan2(dirY, dirX);

            for (int i = 0; i < pellets; i++)
            {
                float angle = baseAngle + (i - (pellets - 1) / 2f) * (spread / (pellets - 1));
                GameWindow.AddProjectile(new Projectile(x, y, (float)Math.Cos(angle), (float)Math.Sin(angle), ProjectileSize, Damage, ProjectileRange));
            }
        }

        public override void SecondaryShoot(float x, float y, float dirX, float dirY)
        {
            if (CurrentAmmo <= 0) return;

            int ammoSpent = CurrentAmmo; 
            CurrentAmmo = 0;

            int superDamage = ammoSpent ;
            int superSize = 10 + (ammoSpent * 4);

            GameWindow.AddProjectile(new Projectile(x, y, dirX, dirY, superSize, superDamage));
        }
    }
}