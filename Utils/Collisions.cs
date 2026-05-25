using game.core;
using game.Entities;
using game.Weapons;
using System;
using System.Drawing;

namespace game.Utils
{
    internal class Collisions
    {
        public static void PlayerCollidesWithEnemy(Player player, Enemy enemy)
        {
            if (!player.IsAlive || !enemy.IsAlive) return;

            player.TakeDamage((int)((enemy.CollisionDamageDeal*player.CollisionDamageTakeMultiplier - player.CollisionDamageTakeReduction)*player.AllDamageTakeMultiplier));
            enemy.TakeDamage((int)((player.CollisionDamageDeal*enemy.CollisionDamageTakeMultiplier - enemy.CollisionDamageTakeReduction)*enemy.AllDamageTakeMultiplier));

            float dx = player.X - enemy.X;
            float dy = player.Y - enemy.Y;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);

            if (length < 0.1f) { dx = 1; dy = 0; length = 1; }

            dx /= length;
            dy /= length;

            float force = (player.CollisionKnockback + enemy.CollisionKnockback) * 0.5f;

            player.ApplyKnockback(dx * force, dy * force);
            enemy.ApplyKnockback(-dx * force, -dy * force);
        }

        public static void ProjectileKnockback(Projectile projectile, GameObject obj)
        {
            float dx = obj.X - projectile.X;
            float dy = obj.Y - projectile.Y;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);

            if (length < 0.1f) { dx = 1; dy = 0; length = 1; }

            dx /= length;
            dy /= length;

            float force = obj.ProjectileKnockback;

            if (obj is Enemy enemy)
            {
                enemy.ApplyKnockback(dx * force, dy * force);
            }
            else if (obj is Player player)
            {
                player.ApplyKnockback(dx * force, dy * force);
            }
        }
    }
}