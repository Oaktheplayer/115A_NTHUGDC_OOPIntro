using UnityEngine;

namespace Combat
{
    [RequireComponent(
        typeof(Damager),
        typeof(RangeDetect)
        )]
    public class Projectile : MonoBehaviour
    {
        private delegate ProjectileMovement MovementFactory(Projectile projectile);

        private static readonly MovementFactory[] StrategyFactories =
        {
            p => new HomingMovement(p),
            p => new StraightMovement(p),
            p => new HomingAccelMovement(p),
            p => new StraightRBMovement(p),
        };

        [SerializeField] private ProjectileMovementType movementType;
        [SerializeField] private float speed = 5f;
        [SerializeField] private float accel = 5f;
        [SerializeField] private float maxLifetime = 10f;
        [SerializeField] private bool removeUponCollision;

        public float Speed => speed;
        public float Accel => accel;

        private Damager damager;
        private RangeDetect rangeDetect;
        private ProjectileMovement movement;
        private float spawnTime;

        public Transform Target { get; private set; }
        public Vector2 direction;

        private void Awake()
        {
            damager = GetComponent<Damager>();
            rangeDetect = GetComponent<RangeDetect>();
            rangeDetect.OnTargetDetected += OnTargetDetected;
        }

        private void OnValidate()
        {
            if ((int)movementType < 0 || (int)movementType >= StrategyFactories.Length)
                movementType = ProjectileMovementType.Homing;
        }

        public void Initialize(Transform target)
        {
            Initialize((Vector2)(target.position - transform.position),target);
        }

        public void Initialize(Vector2 direction, Transform target = null)
        {
            Target = target;
            spawnTime = Time.time;
            this.direction = direction.normalized;
            movement = StrategyFactories[(int)movementType](this);
            FaceDirection(direction);

            movement.Start();
        }

        public void FaceDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
                return;

            transform.up = direction;
        }

        private void FixedUpdate()
        {

            if (movement.DoStop())
            {
                Despawn();
                return;
            }

            if (Time.time - spawnTime >= maxLifetime)
            {
                Despawn();
                return;
            }

            movement.Step(speed, Time.fixedDeltaTime);
        }

        private void OnTargetDetected()
        {
            for(int i = 0; i < rangeDetect.Targets.Count; i++)
            {
                Transform target = rangeDetect.Targets[i];
                if (target.TryGetComponent(out Health health))
                {
                    damager.DealDamage(health);
                }
            }    
            
            // clean up self
            Despawn();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!removeUponCollision) { return; }

            Despawn();
        }

        public void Despawn()
        {
            Destroy(gameObject);
        }
    }
}
