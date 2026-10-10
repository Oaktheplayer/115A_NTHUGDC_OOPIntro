using Combat;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// Applies <see cref="PlayerData"/> to player components at runtime (mirrors EnemyController + EnemyData).
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerData data;
        [SerializeField] private Health health;
        [SerializeField] private PlayerMover mover;

        private void Awake()
        {
            if (health == null)
                health = GetComponent<Health>();
            if (mover == null)
                mover = GetComponent<PlayerMover>();

            ApplyData();
        }

        private void ApplyData()
        {
            if (data == null)
                return;

            if (health != null)
                health.ApplyMaxHealth(data.maxHealth);

            if (mover != null)
                mover.ApplyMoveSpeed(data.moveSpeed);
        }
    }
}
