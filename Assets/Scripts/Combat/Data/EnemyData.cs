using UnityEngine;

namespace Combat
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Combat/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Min(1)] public int maxHealth = 3;
        [Min(0f)] public float moveSpeed = 2.5f;
        [Min(0.1f)] public float aggroRange = 5f;
    }
}
