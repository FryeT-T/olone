using game.core;
using game.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace game.Items.Guns
{
    internal class SniperRifle : Weapon
    {
        public SniperRifle()
        {
            Name = "Снайперская винтовка";
            ShootDelay = 1.5f;
            SecondaryShootDelay = 1.5f;
            MaxAmmo = 5;
            CurrentAmmo = 5;
            ReloadTime = 1.8f;
            Damage = 10;
            ProjectileSize = 6;
            ProjectileRange = 1000;
        }

        public override void Shoot(float x, float y, float dirX, float dirY)
        {
            if (CurrentAmmo <= 0) return;
            CurrentAmmo--;
            GameWindow.AddProjectile(new Projectile(x, y, dirX, dirY, ProjectileSize, Damage, ProjectileRange));
        }
    }
}
