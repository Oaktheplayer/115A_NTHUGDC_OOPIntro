using Combat;
using UnityEngine;

namespace Enemy
{
    public class EnemyController : MonoBehaviour
    {
        private enum State
        {
            Idle,
            Aggro
        }

        [SerializeField] private EnemyData data;
        [SerializeField] private EnemyMovement movement;
        [SerializeField] private AggroZone aggroZone;
        [SerializeField] private WeaponPivot weaponPivot;
        [SerializeField] private Weapon weapon;
        [SerializeField] private Health health;

        private State state = State.Idle;
        private Transform player;

        private void Awake()
        {
            if (movement == null)
                movement = GetComponent<EnemyMovement>();
            if (health == null)
                health = GetComponent<Health>();
            if (weaponPivot == null)
                weaponPivot = GetComponentInChildren<WeaponPivot>(true);
            if (weapon == null)
                weapon = GetComponentInChildren<Weapon>(true);
            if (aggroZone == null)
                aggroZone = GetComponentInChildren<AggroZone>(true);

            // TODO [LAB-H2] 把 EnemyData（ScriptableObject）裡的血量和仇恨範圍，套用到 health 與 aggroZone 上。
            // 提示：資料集中在一個地方（data），其他元件再去讀——看看 Health 和 AggroZone 各有哪個 public 方法可以接收這些數值。

            if (weapon != null && weaponPivot != null)
                weaponPivot.Equip(weapon, WeaponOwner.EnemyTag);
            else if (weapon != null)
                weapon.EquipTo(transform, WeaponOwner.EnemyTag);

            movement?.Stop();
            EnemyRegistry.Register(this);
        }

        private void OnDestroy()
        {
            EnemyRegistry.Unregister(this);
        }

        public void NotifyPlayerDetected(Transform playerTransform)
        {
            if (state == State.Aggro)
                return;

            player = playerTransform;
            state = State.Aggro;
        }

        private void LateUpdate()
        {
            if (state != State.Aggro || player == null || weaponPivot == null)
                return;

            Vector2 direction = (Vector2)player.position - (Vector2)transform.position;
            weaponPivot.FaceDirection(direction);
        }

        private void FixedUpdate()
        {
            if (state != State.Aggro || player == null)
            {
                movement?.Stop();
                return;
            }

            movement.MoveTowards(player.position, data.moveSpeed);

            Weapon equipped = weaponPivot != null ? weaponPivot.CurrentWeapon : weapon;
            if (equipped != null && equipped.IsReady)
            {
                Vector2 direction = (Vector2)player.position - (Vector2)transform.position;
                if (direction.sqrMagnitude > 0.0001f)
                    equipped.Attack(direction.normalized, player);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (data != null && aggroZone != null)
                aggroZone.SetRadius(data.aggroRange);
        }
#endif
    }
}
