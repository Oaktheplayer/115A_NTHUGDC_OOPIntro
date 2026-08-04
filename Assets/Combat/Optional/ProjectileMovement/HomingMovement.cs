using System;
using UnityEngine;

namespace Combat
{
    public class HomingMovement : ProjectileMovement
    {
        public HomingMovement(Projectile projectile) : base(projectile) { }

        public override void Step(float speed, float deltaTime)
        {
            var target = Projectile.Target;
            if (target == null)
                return;

            var transform = Projectile.transform;
            var direction = (Vector2)(target.position - transform.position);
            Projectile.FaceDirection(direction);

            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                speed * deltaTime);
        }
    }
}
