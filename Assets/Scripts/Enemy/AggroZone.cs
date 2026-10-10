using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// Trigger zone that notifies the EnemyController when the player enters.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class AggroZone : MonoBehaviour
    {
        [SerializeField] private EnemyController controller;
        [SerializeField] private CircleCollider2D circle;

        private void Awake()
        {
            if (controller == null)
                controller = GetComponentInParent<EnemyController>();
            if (circle == null)
                circle = GetComponent<CircleCollider2D>();

            circle.isTrigger = true;
        }

        public void SetRadius(float radius)
        {
            if (circle == null)
                circle = GetComponent<CircleCollider2D>();
            circle.radius = Mathf.Max(0.1f, radius);
        }

        // TODO [LAB-H1] 自己寫出 Unity 的 Trigger 回呼方法：玩家走進圈圈時，用 CompareTag(Combat.WeaponOwner.PlayerTag) 確認是玩家，再呼叫 controller.NotifyPlayerDetected(other.transform)。
        // 提示：上一堂 Physics2D——圈圈的 Collider2D 已設 isTrigger，所以要用 OnTrigger 系列而不是 OnCollision 系列；方法名稱與參數型別（2D 版！）拼錯 Unity 就不會呼叫。
        // 提示：至少一方要有 Rigidbody2D 才會觸發（敵人身上已經有了）；這裡是 2D，參數是 Collider2D 不是 Collision2D。

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (circle == null)
                circle = GetComponent<CircleCollider2D>();
            if (circle == null)
                return;

            Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, circle.radius * Mathf.Abs(transform.lossyScale.x));
        }
#endif
    }
}
