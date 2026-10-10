using UnityEngine;

namespace Combat
{
    public class HomingAccelMovement : AccelMovement
    {
        public HomingAccelMovement(Projectile projectile) : base(projectile) { }

        public override void Step(float speed, float deltaTime)
        {
            var target = Projectile.Target;
            if (target == null)
                return;

            var position = rigidbody2D.position;
            Vector2 offset = (Vector2)target.position - position;
            var direction = offset.normalized;
            Projectile.FaceDirection(direction);

            Vector2 desiredVelocity = offset.normalized * maxSpeed;
            Vector2 desiredAccel = (desiredVelocity - rigidbody2D.linearVelocity) / deltaTime;
            Vector2 accel = Vector2.ClampMagnitude(desiredAccel, maxAccel);

            rigidbody2D.AddForce(accel * mass, ForceMode2D.Force);
        }
    }
}
