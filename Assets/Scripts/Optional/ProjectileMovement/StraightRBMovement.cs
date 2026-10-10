using UnityEngine;

namespace Combat
{
    public class StraightRBMovement : AccelMovement
    {
        private readonly Vector2 direction;
        public StraightRBMovement(Projectile projectile) : base(projectile)
        {
            direction = projectile.direction.normalized;
        }

        public override void Start()
        {
            rigidbody2D.linearVelocity = direction * maxSpeed;
        }
        public override void Step(float speed, float deltaTime)
        {
            base.Step(speed, deltaTime);
        }
    }
}
