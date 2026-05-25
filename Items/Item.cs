using System.Drawing;

namespace game.Items
{
    public abstract class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public abstract class Weapon : Item
    {
        public float ShootDelay { get; set; }
        public float SecondaryShootDelay { get; set; }
        public int Damage { get; set; }
        public int ProjectileSize { get; set; }
        public float ProjectileRange { get; set; }
        public int MaxAmmo { get; set; }
        public int CurrentAmmo { get; set; }
        public float ReloadTime { get; set; }
        public float ReloadTimer { get; set; } 
        public bool IsReloading { get; set; }

        public abstract void Shoot(float x, float y, float dirX, float dirY);
        public virtual void SecondaryShoot(float x, float y, float dirX, float dirY) { }

        public void StartReload()
        {
            if (CurrentAmmo < MaxAmmo && !IsReloading)
            {
                IsReloading = true;
                ReloadTimer = ReloadTime;
            }
        }

        public void UpdateReload(float dt)
        {
            if (IsReloading)
            {
                ReloadTimer -= dt;
                if (ReloadTimer <= 0)
                {
                    CurrentAmmo = MaxAmmo;
                    IsReloading = false;
                }
            }
        }

        public void CancelReload()
        {
            IsReloading = false;
            ReloadTimer = 0;
        }
    }
}