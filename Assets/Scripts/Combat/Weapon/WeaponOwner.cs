using UnityEngine;

namespace Combat
{
    /// <summary>
    /// Tracks who holds the weapon and which tag it is allowed to damage.
    /// Player-held weapons target "Enemy"; enemy-held weapons target "Player".
    /// </summary>
    public class WeaponOwner : MonoBehaviour
    {
        public const string PlayerTag = "Player";
        public const string EnemyTag = "Enemy";

        [SerializeField] private string validTargetTag = EnemyTag;

        public string ValidTargetTag => validTargetTag;
        public string HolderTag { get; private set; } = PlayerTag;

        public void ConfigureForHolder(string holderTag)
        {
            HolderTag = holderTag;
            validTargetTag = holderTag == PlayerTag ? EnemyTag : PlayerTag;
        }
    }
}
