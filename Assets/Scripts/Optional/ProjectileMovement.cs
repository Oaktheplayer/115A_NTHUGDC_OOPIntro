namespace Combat
{
    public abstract class ProjectileMovement
    {
        protected Projectile Projectile { get; }

        protected ProjectileMovement(Projectile projectile)
        {
            Projectile = projectile;
        }

        public virtual void Start() { }
        public virtual void Step(float speed, float deltaTime) { }

        public virtual bool DoStop() { return false; }
    }
}
