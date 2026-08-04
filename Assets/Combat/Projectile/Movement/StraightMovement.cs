using System;
using UnityEngine;

namespace Combat
{
    public class StraightMovement : ProjectileMovement
    {
        private readonly Vector2 direction;

        public StraightMovement(Projectile projectile) : base(projectile)
        {
            direction = ((Vector2)(projectile.Target.position - projectile.transform.position)).normalized;
        }

        public override void Step(float speed, float deltaTime)
        {
            Projectile.FaceDirection(direction);
            Projectile.transform.position += (Vector3)(direction * speed * deltaTime);
        }
    }
}
