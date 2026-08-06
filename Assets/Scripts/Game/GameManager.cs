using Combat;
using Enemy;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        private enum State
        {
            Playing,
            Paused,
            GameOver
        }

        [Header("Panels")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private CanvasGroup gameOverCanvasGroup;
        [SerializeField] private TMP_Text gameOverTitle;

        [Header("Buttons")]
        [SerializeField] private Button pauseRestartButton;
        [SerializeField] private Button gameOverRestartButton;

        [Header("Game Over Fade")]
        [SerializeField] private float gameOverFadeSmoothTime = 0.4f;

        [Header("Debug")]
        [Tooltip("If true, Game Over does not show / fade the GameOver panel.")]
        [SerializeField] private bool skipGameOverPanel;
        [Tooltip("If true, Game Over does not set timeScale to 0.")]
        [SerializeField] private bool skipPauseOnGameOver;

        [Header("Copy")]
        [SerializeField] private string winText = "You Win!";
        [SerializeField] private string loseText = "You Lose...";

        private State state = State.Playing;
        private InputAction pauseAction;
        private Health playerHealth;
        private bool fadingGameOver;
        private float gameOverAlphaVelocity;

        private void Awake()
        {
            Time.timeScale = 1f;

            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            if (gameOverCanvasGroup != null)
                gameOverCanvasGroup.alpha = 0f;

            fadingGameOver = false;
            gameOverAlphaVelocity = 0f;

            pauseAction = FindPauseAction();
        }

        private void OnEnable()
        {
            EnemyRegistry.OnAllEnemiesDefeated += HandleWin;
            BindPlayerHealth();

            if (pauseRestartButton != null)
                pauseRestartButton.onClick.AddListener(Restart);
            if (gameOverRestartButton != null)
                gameOverRestartButton.onClick.AddListener(Restart);

            pauseAction?.Enable();
        }

        private void OnDisable()
        {
            EnemyRegistry.OnAllEnemiesDefeated -= HandleWin;
            if (playerHealth != null)
                playerHealth.OnKilledEvent -= HandleLose;

            if (pauseRestartButton != null)
                pauseRestartButton.onClick.RemoveListener(Restart);
            if (gameOverRestartButton != null)
                gameOverRestartButton.onClick.RemoveListener(Restart);

            pauseAction?.Disable();
        }

        private void Update()
        {
            if (state != State.GameOver && pauseAction != null && pauseAction.WasPressedThisFrame())
                TogglePause();

            if (fadingGameOver)
            {
                if (gameOverCanvasGroup == null)
                {
                    fadingGameOver = false;
                }
                else
                {
                    gameOverCanvasGroup.alpha = Mathf.SmoothDamp(
                        gameOverCanvasGroup.alpha,
                        1f,
                        ref gameOverAlphaVelocity,
                        gameOverFadeSmoothTime,
                        Mathf.Infinity,
                        Time.unscaledDeltaTime);

                    if (gameOverCanvasGroup.alpha >= 0.995f)
                    {
                        gameOverCanvasGroup.alpha = 1f;
                        fadingGameOver = false;
                    }
                }
            }
        }

        public void TogglePause()
        {
            if (state == State.GameOver)
                return;

            if (state == State.Paused)
                Resume();
            else
                Pause();
        }

        public void Pause()
        {
            if (state != State.Playing)
                return;

            state = State.Paused;
            Time.timeScale = 0f;
            if (pausePanel != null)
                pausePanel.SetActive(true);
        }

        public void Resume()
        {
            if (state != State.Paused)
                return;

            state = State.Playing;
            Time.timeScale = 1f;
            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        public void TriggerGameOver(bool won)
        {
            if (state == State.GameOver)
                return;

            state = State.GameOver;

            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (!skipPauseOnGameOver)
                Time.timeScale = 0f;

            if (gameOverTitle != null)
                gameOverTitle.text = won ? winText : loseText;

            if (skipGameOverPanel)
            {
                fadingGameOver = false;
                return;
            }

            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            if (gameOverCanvasGroup != null)
            {
                gameOverCanvasGroup.alpha = 0f;
                gameOverAlphaVelocity = 0f;
                fadingGameOver = true;
            }
            else
                fadingGameOver = false;
        }

        public void Restart()
        {
            fadingGameOver = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void HandleWin()
        {
            TriggerGameOver(won: true);
        }

        private void HandleLose()
        {
            TriggerGameOver(won: false);
        }

        private void BindPlayerHealth()
        {
            if (playerHealth != null)
                playerHealth.OnKilledEvent -= HandleLose;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;

            playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
                playerHealth.OnKilledEvent += HandleLose;
        }

        private static InputAction FindPauseAction()
        {
            // Prefer the project InputActionAsset used by PlayerInput.
            var playerInput = Object.FindFirstObjectByType<PlayerInput>();
            if (playerInput != null && playerInput.actions != null)
            {
                InputAction pause = playerInput.actions.FindAction("Pause", throwIfNotFound: false);
                if (pause != null)
                    return pause;
            }

            foreach (var asset in Resources.FindObjectsOfTypeAll<InputActionAsset>())
            {
                if (asset == null || asset.name != "InputSystem_Actions")
                    continue;
                InputAction pause = asset.FindAction("Pause", throwIfNotFound: false);
                if (pause != null)
                    return pause;
            }

            return null;
        }
    }
}
