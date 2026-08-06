using UnityEngine;

namespace Combat
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Combat/Player Data")]
    public class PlayerData : ScriptableObject
    {
        [Min(1)] public int maxHealth = 5;
        [Min(0f)] public float moveSpeed = 5f;
    }
}
