using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startMenuUI;
    public GameObject pauseUI;
    public GameObject controlsUI;
    public GameObject healthBarUI;

    [Header("Controllers")]
    [SerializeField] private WorldCrossHairController crosshairController;
    [SerializeField] private CinemachineInputAxisController cameraAxisController;
    [SerializeField] private AimCameraController aimCameraController;
    [SerializeField] private CameraSwitcher cameraSwitcher;

    private PlayerActions playerActions;
    private bool hasStarted = false;

    void Awake()
    {
        playerActions = new PlayerActions();
    }

    void Start()
    {
        SetUIState(true, false, false, false, false, CursorLockMode.None);
    }

    void Update()
    {
        if (hasStarted && Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (!pauseUI.activeSelf) onPausePress();
            else OnResumePress();
        }
    }

    public void OnStartPress()
    {
        hasStarted = true;
        SetUIState(false, false, true, true, true, CursorLockMode.Locked);
    }

    public void onPausePress()
    {
        SetUIState(false, true, false, false, false, CursorLockMode.None);
    }

    public void OnResumePress()
    {
        SetUIState(false, false, true, true, true, CursorLockMode.Locked);
    }

    public void OnReturnPress()
    {
        hasStarted = false;
        SetUIState(true, false, false, false, false, CursorLockMode.None);
    }

    public void OnRestartPress()
    {
        System.GC.Collect();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Declares the function and accepts true/false settings plus cursor choices from other methods.
    private void SetUIState(bool start, bool pause, bool controls, bool health, bool gameplayActive, CursorLockMode lockMode)
    {
        startMenuUI.SetActive(start);
        pauseUI.SetActive(pause);
        controlsUI.SetActive(controls);
        healthBarUI.SetActive(health);

        // If the crosshair script exists, it hides it in menus and shows it during gameplay.
        if (crosshairController != null) crosshairController.SetCrosshairVisibility(health);

        Cursor.lockState = lockMode;
        Cursor.visible = (lockMode == CursorLockMode.None);

        // Sets game speed: 1f runs the game normally; 0f completely freezes physics, time, and animations.
        Time.timeScale = gameplayActive ? 1f : 0f;

        // Checks if your Input Action asset instance exists in memory.
        if (playerActions != null)
        {
            if (gameplayActive) playerActions.Enable();
            else playerActions.Disable();
        }

        if (cameraAxisController != null) cameraAxisController.enabled = gameplayActive;
        if (aimCameraController != null) aimCameraController.enabled = gameplayActive;
    }

    public void OnExitPress()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
