using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerAim : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;

        public Vector2 AimDirection { get; private set; } = Vector2.up;

        private void Awake()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;
        }

        private void Update()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;
            if (worldCamera == null)
                return;

            Vector2 screenPos;
            if (Mouse.current == null)
                return;

            screenPos = Mouse.current.position.ReadValue();

            Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(worldCamera.transform.position.z)));
            Vector2 dir = (Vector2)(world - transform.position);
            if (dir.sqrMagnitude > 0.0001f)
                AimDirection = dir.normalized;
        }
    }
}
