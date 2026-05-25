using game.core;
using game.Weapons;

namespace game.Items.Guns
{
    public class Pistol : Weapon
    {
        public Pistol()
        {
            Name = "Пистолет";
            Description = "Стандартное полуавтоматическое оружие.";

            ShootDelay = 0.3f;
            Damage = 1;
            ProjectileSize = 8;

            MaxAmmo = 12;
            CurrentAmmo = 12;
            ReloadTime = 1.2f; 
        }

        public override void Shoot(float x, float y, float dirX, float dirY)
        {
            if (CurrentAmmo > 0)
            {
                CurrentAmmo--;
                GameWindow.AddProjectile(new Projectile(x, y, dirX, dirY, ProjectileSize, Damage));
            }
        }

    }
}