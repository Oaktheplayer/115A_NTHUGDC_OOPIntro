using System;
using UnityEngine;

namespace Combat
{
    [Serializable]
    public abstract class AccelMovement : ProjectileMovement
    {
        readonly protected float maxAccel = 5f;
        readonly protected float maxSpeed = 5f;
        readonly protected float mass = 1f;

        protected Rigidbody2D rigidbody2D;
        public AccelMovement(Projectile projectile) : base(projectile)
        {
            rigidbody2D = projectile.GetComponent<Rigidbody2D>();
            maxAccel = projectile.Accel;
            maxSpeed = projectile.Speed;
            mass = rigidbody2D.mass;
        }
    }
}
